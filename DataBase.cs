using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewVariant.Interfaces;
using NewVariant.Models;
using NewVariant.Exceptions;
using System.Text.Json;
using System.Security.Principal;

namespace DataBaseLibrary
{
    /// <summary>
    /// Класс для работы с базой данных
    /// </summary>
    public class DataBase : IDataBase
    {
        //Базы данных
        public static List<IEntity> buyers;
        public static List<IEntity> goods;
        public static List<IEntity> sales;
        public static List<IEntity> shops;
        public List<List<IEntity>> tables = new List<List<IEntity>> { buyers, goods, sales, shops };

        //Конструктор без параметров для создания объекта
        public DataBase() { }

        /// <summary>
        /// Создает таблицу и проверяет, что она еще не создана
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="DataBaseException"></exception>
        public void CreateTable<T>() where T : IEntity
        {
            int num = GetTableIndex<T>();
            if (tables[num] == null)
                tables[num] = new List<IEntity> { };
            else
                throw new DataBaseException("Таблица существует");
        }

        /// <summary>
        /// Десериализует из json
        /// </summary>
        /// <typeparam name="T">Тип таблицы</typeparam>
        /// <param name="path">путь файла</param>
        /// <exception cref="DataBaseException">Исключение</exception>
        public void Deserialize<T>(string path) where T : IEntity
        {
            string fileText;

            try
            { fileText = File.ReadAllText(path); }
            catch (Exception e)
            {
                throw new DataBaseException($"\nПри попытке открыть файл произошла ошибка: \n{e.Message}");
            }

            try
            {
                List<T> deSerialize = JsonSerializer.Deserialize<List<T>>(fileText);
                tables[GetTableIndex<T>()] = deSerialize.Cast<IEntity>().ToList();
            }
            catch (Exception e)
            {
                throw new DataBaseException($"\nВо время десиреализации произошла ошибка: \n{e.Message}");
            }
        }

        /// <summary>
        /// Метод, возвращающий таблицу типа T
        /// </summary>
        /// <typeparam name="T">тип</typeparam>
        /// <returns>Таблицу</returns>
        /// <exception cref="DataBaseException">Исключение</exception>
        public IEnumerable<T> GetTable<T>() where T : IEntity
        {
            if (tables[GetTableIndex<T>()] == null)
            { throw new DataBaseException("Данной таблицы не существует!"); }

            try
            { return (IEnumerable<T>)tables[GetTableIndex<T>()]; }
            catch (Exception e)
            { throw new DataBaseException($"Ошибка! \n{e.Message}"); }
        }

        /// <summary>
        /// Метод, вставляющий значеие делегата в таблицу типа T
        /// </summary>
        /// <typeparam name="T">Тип</typeparam>
        /// <param name="getEntity">делегат со значением</param>
        /// <exception cref="DataBaseException">Исключение</exception>
        public void InsertInto<T>(Func<T> getEntity) where T : IEntity
        {
            if (tables[GetTableIndex<T>()] == null)
                throw new DataBaseException("Ошибка! Попытка заполнить несуществующую таблицу");

            try
            { tables[GetTableIndex<T>()].Add(getEntity()); }
            catch (Exception e)
            { throw new DataBaseException($"Ошибка: \n{e.Message}"); }
        }

        /// <summary>
        /// Метод сериализации
        /// </summary>
        /// <typeparam name="T">Тип</typeparam>
        /// <param name="path">Путь файла</param>
        /// <exception cref="DataBaseException">Исключение</exception>
        public void Serialize<T>(string path) where T : IEntity
        {
            List<T> toSerialize = new List<T> { };

            if (tables[GetTableIndex<T>()] == null)
                throw new DataBaseException("Попытка сериализовать пустую таблицу");

            try
            {
                toSerialize = tables[GetTableIndex<T>()].Cast<T>().ToList();
                File.WriteAllText(path, JsonSerializer.Serialize(toSerialize));
            }
            catch (Exception e)
            { throw new DataBaseException($"Ошибка! \n{e.Message}"); }
        }

        /// <summary>
        /// Возвращает индекс таблицы по ее типу (вспомогательный метод)
        /// </summary>
        /// <typeparam name="T">Тип</typeparam>
        /// <returns>Индекс (число)</returns>
        /// <exception cref="DataBaseException">Исключение</exception>
        public int GetTableIndex<T>()
        {
            string tableType;

            try
            {
                List<T> values = new List<T>();
                tableType = values.GetType().GetGenericArguments()[0].ToString();
            }
            catch (Exception e)
            { throw new DataBaseException($"Ошибка: {e.Message}"); }

            if (tableType == "NewVariant.Models.Buyer")
            {
                return 0;
            }
            else if (tableType == "NewVariant.Models.Good")
            {
                return 1;
            }
            else if (tableType == "NewVariant.Models.Sale")
            {
                return 2;
            }
            else if (tableType == "NewVariant.Models.Shop")
            {
                return 3;
            }
            else
                throw new DataBaseException($"Тип таблицы {tableType} не является допустимым для этого метода");
        }

        /// <summary>
        /// Получает тип таблицы (вспомогательный метод)
        /// </summary>
        /// <typeparam name="T">Тип</typeparam>
        /// <returns>Тип</returns>
        public Type GetTableType<T>()
        {
            return (new List<T>()).GetType();
        }
    }
}
