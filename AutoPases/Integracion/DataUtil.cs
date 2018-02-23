using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace AutoPases.Integracion
{
    public static class DataUtil
    {
        // function that set the given object from the given data row
        public static void SetItemFromRow<T>(T item, DataRow row) where T : new()
        {
            // go through each column
            foreach (DataColumn c in row.Table.Columns)
            {
                // find the property for the column
                PropertyInfo p = item.GetType().GetProperty(c.ColumnName);

                // if exists, set the value
                if (p != null && row[c] != DBNull.Value)
                {
                    p.SetValue(item, row[c], null);
                }
            }
        }

        // function that creates an object from the given data row
        public static T CreateItemFromRow<T>(this DataRow row) where T : new()
        {
            // create a new object
            T item = new T();

            // set the item
            SetItemFromRow(item, row);

            // return 
            return item;
        }


        // function that creates a list of an object from the given data table
        public static List<T> CreateListFromTable<T>(this DataTable tbl) where T : new()
        {
            // define return list
            List<T> lst = new List<T>();

            // go through each row
            foreach (DataRow r in tbl.Rows)
            {
                // add to the list
                lst.Add(CreateItemFromRow<T>(r));
            }

            // return the list
            return lst;
        }
        public static T CreateItemFromReader<T>(this IDataReader dr) where T : new()
        {
            T item = new T();
            item = Activator.CreateInstance<T>();
            for (int i = 0; i < dr.FieldCount; i++)
            {
                var column = dr.GetName(i);
                PropertyInfo p = item.GetType().GetProperty(column);

                // if exists, set the value
                if (p != null && dr[column] != DBNull.Value)
                {
                    p.SetValue(item, dr[column], null);
                }
            }

            return item;
        }
        public static List<T> DataReaderMapToList<T>(this IDataReader dr)
        {
            List<T> list = new List<T>();
            T obj = default(T);
            while (dr.Read())
            {
                obj = Activator.CreateInstance<T>();
                foreach (PropertyInfo prop in obj.GetType().GetProperties())
                {
                    if (dr[prop.Name] != null)
                    {
                        if (!object.Equals(dr[prop.Name], DBNull.Value))
                        {
                            prop.SetValue(obj, dr[prop.Name], null);
                        }
                    }
                }
                list.Add(obj);
            }
            return list;
        }
    }
}