#region License

// Copyright © 2013 Marc Gravell, Iker Celorrio, Dyatchenko
// Copyright © 2019 Wesley Hamilton
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of 
// this software and associated documentation files (the "Software"), to deal in the 
// Software without restriction, including without limitation the rights to use, 
// copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the
// Software, and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all 
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN
// AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#endregion

using System.Collections;
using System.Data;
using System.Data.Common;
using System.Reflection;

namespace Fireflies.Utility.Reflection.Fasterflect; 

/// <summary>
///     Creates <see cref="DbDataReader" /> objects for the given type.
/// </summary>
public class DataReaderFactory {
    internal Dictionary<string, int> IndexMap = new();

    /// <summary>
    ///     Constructs a factory for generating <see cref="DbDataReader" />s of the given type.
    /// </summary>
    /// <param name="type">The declaring type of the properties/fields.</param>
    /// <param name="flags">The <see cref="BindingFlags" /> </param>
    public DataReaderFactory(Type type, BindingFlags flags = BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public)
        : this(type, type.GetProperties(flags), type.GetFields(flags)) {
    }

    /// <summary>
    ///     Constructs a factory for generating <see cref="DbDataReader" />s of the given type.
    /// </summary>
    /// <param name="type">The declaring type of the properties/fields.</param>
    /// <param name="properties">The properties that will be mapped by the <see cref="DbDataReader" />.</param>
    public DataReaderFactory(Type type, IEnumerable<PropertyInfo> properties) : this(type, properties, Constants.EmptyFieldInfoArray) {
    }

    /// <summary>
    ///     Constructs a factory for generating <see cref="DbDataReader" />s of the given type.
    /// </summary>
    /// <param name="type">The declaring type of the properties/fields.</param>
    /// <param name="properties">The properties that will be mapped by the <see cref="DbDataReader" />.</param>
    /// <param name="fields">The fields that will be mapped by the <see cref="DbDataReader" />.</param>
    public DataReaderFactory(Type type, IEnumerable<PropertyInfo> properties, IEnumerable<FieldInfo> fields) {
        Type = type;
        Members = properties.Concat<MemberInfo>(fields).ToArray();
        MemberNames = new string[Members.Length];
        MemberTypes = new Type[Members.Length];
        Getters = new MemberGetter[Members.Length];
        AllowNull = new BitArray(Members.Length);
        for(var i = 0; i < Members.Length; ++i) {
            var member = Members[i];
            MemberNames[i] = member.Name;
            Getters[i] = Reflect.Getter(member);
            var memberType = member is PropertyInfo pi ? pi.PropertyType : (member as FieldInfo).FieldType;
            var underlying = Nullable.GetUnderlyingType(memberType);
            AllowNull[i] = !memberType.IsValueType || underlying != null;
            MemberTypes[i] = underlying ?? memberType;
            IndexMap.Add(member.Name, i);
        }
    }

    /// <summary>
    ///     The type of objects that can be passed as a list to <see cref="Create(IEnumerable{object})" />.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    ///     The properties and fields that will be visible by the <see cref="DbDataReader" />.
    /// </summary>
    public MemberInfo[] Members { get; }

    private MemberGetter[] Getters { get; }
    private BitArray AllowNull { get; }
    private string[] MemberNames { get; }
    private Type[] MemberTypes { get; }

    /// <summary>
    ///     Creates a <see cref="DbDataReader" /> for a given data source.
    /// </summary>
    /// <param name="list">The data source for the <see cref="DbDataReader" />.</param>
    /// <returns>The <see cref="DbDataReader" /> for the given data source.</returns>
    public DbDataReader Create(IEnumerable<object> list) {
        return new DataReader(list.GetEnumerator(), this);
    }

    /// <summary>
    ///     Creates <see cref="DbDataReader" /> objects for the given type.
    /// </summary>
    /// <remarks>https://github.com/mgravell/fast-member/blob/master/FastMember/ObjectReader.cs</remarks>
    internal class DataReader : DbDataReader {
        private readonly BitArray AllowNull;
        private readonly MemberGetter[] Getters;
        private readonly Dictionary<string, int> IndexMap;
        private readonly Type[] MemberTypes;
        private readonly string[] PropertyNames;
        private object Current;

        private IEnumerator Enumerator;

        internal DataReader(IEnumerator enumerator, DataReaderFactory factory) {
            Enumerator = enumerator;
            Getters = factory.Getters;
            PropertyNames = factory.MemberNames;
            MemberTypes = factory.MemberTypes;
            AllowNull = factory.AllowNull;
            IndexMap = factory.IndexMap;
        }

        public override object this[int ordinal] => Getters[ordinal](Current) ?? DBNull.Value;

        public override object this[string name] => Getters[IndexMap[name]](Current) ?? DBNull.Value;

        public override int Depth => 0;

        public override int FieldCount => PropertyNames.Length;

        public override bool HasRows => Enumerator != null;

        public override bool IsClosed => Enumerator == null;

        public override int RecordsAffected => 0;

        public override bool GetBoolean(int ordinal) {
            return (bool)this[ordinal];
        }

        public override byte GetByte(int ordinal) {
            return (byte)this[ordinal];
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length) {
            var s = (byte[])this[ordinal];
            var available = s.Length - (int)dataOffset;
            if(available <= 0)
                return 0;

            var count = Math.Min(length, available);
            Buffer.BlockCopy(s, (int)dataOffset, buffer, bufferOffset, count);
            return count;
        }

        public override char GetChar(int ordinal) {
            return (char)this[ordinal];
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length) {
            var s = GetString(ordinal);
            var available = s.Length - (int)dataOffset;
            if(available <= 0)
                return 0;

            var count = Math.Min(length, available);
            s.CopyTo((int)dataOffset, buffer, bufferOffset, count);
            return count;
        }

        public override string GetDataTypeName(int ordinal) {
            return MemberTypes[ordinal].Name;
        }

        public override DateTime GetDateTime(int ordinal) {
            return (DateTime)this[ordinal];
        }

        public override decimal GetDecimal(int ordinal) {
            return (decimal)this[ordinal];
        }

        public override double GetDouble(int ordinal) {
            return (double)this[ordinal];
        }

        public override IEnumerator GetEnumerator() {
            return new DbEnumerator(this);
        }

        public override Type GetFieldType(int ordinal) {
            return MemberTypes[ordinal];
        }

        public override float GetFloat(int ordinal) {
            return (float)this[ordinal];
        }

        public override Guid GetGuid(int ordinal) {
            return (Guid)this[ordinal];
        }

        public override short GetInt16(int ordinal) {
            return (short)this[ordinal];
        }

        public override int GetInt32(int ordinal) {
            return (int)this[ordinal];
        }

        public override long GetInt64(int ordinal) {
            return (long)this[ordinal];
        }

        public override string GetName(int ordinal) {
            return PropertyNames[ordinal];
        }

        public override int GetOrdinal(string name) {
            return IndexMap[name];
        }

        public override string GetString(int ordinal) {
            return (string)this[ordinal];
        }

        public override object GetValue(int ordinal) {
            return this[ordinal];
        }

        public override int GetValues(object[] values) {
            for(int i = 0, count = PropertyNames.Length; i < count; ++i) values[i] = Getters[i](Current) ?? DBNull.Value;
            return PropertyNames.Length;
        }

        public override bool IsDBNull(int ordinal) {
            return this[ordinal] is DBNull;
        }

        public override bool NextResult() {
            return Read();
        }

        public override bool Read() {
            if(Enumerator != null) {
                if(Enumerator.MoveNext()) {
                    Current = Enumerator.Current;
                    return true;
                }

                Enumerator = null;
            }

            return false;
        }

        public override DataTable GetSchemaTable() {
            // these are the columns used by DataTable load
            var table = new DataTable {
                Columns = {
                    { "ColumnOrdinal", typeof(int) },
                    { "ColumnName", typeof(string) },
                    { "DataType", typeof(Type) },
                    { "ColumnSize", typeof(int) },
                    { "AllowDBNull", typeof(bool) }
                }
            };
            var rowData = new object[5];
            for(var i = 0; i < PropertyNames.Length; ++i) {
                rowData[0] = i;
                rowData[1] = PropertyNames[i];
                rowData[2] = MemberTypes[i];
                rowData[3] = -1;
                rowData[4] = AllowNull[i];
                table.Rows.Add(rowData);
            }

            return table;
        }

        public override void Close() {
            Current = null;
            if(Enumerator is IDisposable d) d.Dispose();
            Enumerator = null;
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            if(disposing)
                Close();
        }
    }
}