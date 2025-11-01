using System.Data;

namespace Utils
{
   public static class DataReaderMapper
    {
        public static T MapToObject<T>(this IDataRecord record) where T : new()
        {
            var obj = new T();
            var objType = typeof(T);

            for (int i = 0; i < record.FieldCount; i++)
            {
                string columnName = record.GetName(i);
                var prop = objType.GetProperty(columnName);
                if (prop != null && !record.IsDBNull(i))
                {
                    prop.SetValue(obj, record.GetValue(i));
                }
            }

            return obj;
        }
    }
}