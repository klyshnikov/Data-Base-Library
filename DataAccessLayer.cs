using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using NewVariant.Interfaces;
using NewVariant.Models;
using NewVariant.Exceptions;

namespace DataBaseLibrary
{
    /// <summary>
    /// Класс для получения специальных данных из БД
    /// Для всех метод вызовем метод CheckTableExist, который проверяет, что все таблицы существуют
    /// (для данного метода)
    /// </summary>
    public class DataAccessLayer : IDataAccessLayer
    {
        //Конструктор без параметров
        public DataAccessLayer() { }

        // Получает список товаров, купленных покупателем с самым большим именем
        IEnumerable<Good> IDataAccessLayer.GetAllGoodsOfLongestNameBuyer(IDataBase dataBase)
        {
            CheckTableExist(new List<int> { 0, 1, 2 }, dataBase);

            try
            {
                //Получает id нашего покупателя
                int IdBuyerWithLongestName = ((dataBase as DataBase).tables[0].OrderBy(s => (s as Buyer).Name).Aggregate
                ((x, y) => (y as Buyer).Name.Length >= (x as Buyer).Name.Length ? y : x) as Buyer).Id;

                //Получает список id товаров, которые он купил
                List<int> goodsId = (dataBase as DataBase).tables[2].Where
                    (sale => (sale as Sale).BuyerId == IdBuyerWithLongestName).Select(sale => (sale as Sale).GoodId).ToList();

                //Для каждого товара соединяет списки
                IEnumerable<IEntity> listGoods = new List<IEntity> { };

                foreach (int goodid in goodsId)
                { listGoods = listGoods.Concat((dataBase as DataBase).tables[1].Where(good => (good as Good).Id == goodid)); }

                return listGoods.Cast<Good>().ToList();
            }
            catch (Exception e)
            { throw new DataBaseException($"Неизвестная ошибка! \n{e.Message}"); }
        }

        // Получает минимальное количество магазинов в стране среди всех стран
        int IDataAccessLayer.GetMinimumNumberOfShopsInCountry(IDataBase dataBase)
        {
            CheckTableExist(new List<int> { 3 }, dataBase);

            try
            {
                //Получает список всех стран
                IEnumerable<string> countries = (dataBase as DataBase).tables[0].Select(buyer => (buyer as Buyer).Country).Distinct();
                int minCountofShops = int.MaxValue;

                //Для каждой считает минимум
                foreach (string country in countries)
                {
                    var numShopsInCounry = (dataBase as DataBase).tables[3].Count(shop => (shop as Shop).Country == country);
                    minCountofShops = Math.Min(minCountofShops, numShopsInCounry);
                }

                return minCountofShops;
            }
            catch (Exception e)
            { throw new DataBaseException($"Неизвестная ошибка! \n{e.Message}"); }

        }

        // Получает город, в котором меньше всего было оборота денег
        string? IDataAccessLayer.GetMinimumSalesCity(IDataBase dataBase)
        {
            CheckTableExist(new List<int> { 0, 1, 2, 3 }, dataBase);

            // Получаем список всех городов
            IEnumerable<string> sities = (dataBase as DataBase).tables[0].Select(buyer => (buyer as Buyer).City).Distinct();

            string sityWithMaxPrice = null;
            long minSalesInSity = long.MaxValue;

            foreach (string sity in sities)
            {
                // Пояснение:
                // long a = (dataBase as DataBase).tables[2].Where( ... ):
                //     Тут мы выбираем те продажи, которые были совершены в городе sity
                // .Sum( ... ):
                //     У каждой продажи находим ее товар, соответственно цену. Цену умножаем на количество проданного товара. Это все суммируем
                // Т.е мы нашли, сколько денег потрачено в городе sity
                var salesInSity = (dataBase as DataBase).tables[2].Where(sale => ((dataBase as DataBase).tables[3].
                    Find(shop => shop.Id == (sale as Sale).ShopId) as Shop).City == sity);

                var totalSalesInSity = salesInSity.Sum(x => (((dataBase as DataBase).tables[1].Find(good => good.Id == (x as Sale).GoodId)) as Good).Price * (x as Sale).GoodCount);

                if (totalSalesInSity < minSalesInSity)
                {
                    minSalesInSity = totalSalesInSity;
                    sityWithMaxPrice = sity;
                }
            }

            return sityWithMaxPrice;

        }

        // Получает товар с наибольшей ценой
        string? IDataAccessLayer.GetMostExpensiveGoodCategory(IDataBase dataBase)
        {
            CheckTableExist(new List<int> { 1 }, dataBase);

            try
            {
                // Среди всех товаров оставляет товар с наибольшей ценой
                string nameOfGoodwithMaxPrice = ((dataBase as DataBase).tables[1].Aggregate((x, y)
                => (x as Good).Price > (y as Good).Price ? x : y) as Good).Name;

                return nameOfGoodwithMaxPrice;
            }
            catch (Exception e)
            { throw new DataBaseException($"Неизвестная ошибка! \n{e.Message}"); }
        }

        // Получает список покупателей, который покупали самый популярный товар
        IEnumerable<Buyer> IDataAccessLayer.GetMostPopularGoodBuyers(IDataBase dataBase)
        {
            CheckTableExist(new List<int> { 0, 2 }, dataBase);

            try
            {
                // Получаем самый популярный товар
                var mostPopularGood = (dataBase as DataBase).tables[1].Aggregate((x, y) =>
                    (dataBase as DataBase).tables[2].Sum(sale => (sale as Sale).GoodId == (x as Good).Id ? (sale as Sale).GoodCount : 0)
                    > (dataBase as DataBase).tables[2].Sum(sale => (sale as Sale).GoodId == (y as Good).Id ? (sale as Sale).GoodCount : 0)
                    ? x : y);

                // Покупки, в которых участвовал этот товар
                var listSalesWithPopularGood = (dataBase as DataBase).tables[2].Where(sale => (sale as Sale).GoodId == mostPopularGood.Id);

                // По списку получаем набор покупателей, замешанных в покупке этого товара
                var listBuyersWithPopularGood = (dataBase as DataBase).tables[0].
                    Where(buyer => listSalesWithPopularGood.Any(sale => (sale as Sale).BuyerId == buyer.Id)).Cast<Buyer>();

                return listBuyersWithPopularGood;
            }
            catch (Exception e)
            { throw new DataBaseException($"Неизвестная ошибка! \n{e.Message}"); }
        }

        // Получает список покупок, которые покупались не в родных городах покупателей
        IEnumerable<Sale> IDataAccessLayer.GetOtherCitySales(IDataBase dataBase)
        {
            CheckTableExist(new List<int> { 0, 2, 3 }, dataBase);

            try
            {
                return (dataBase as DataBase).tables[2].Where
                (sale => ((dataBase as DataBase).tables[0].Find(buyer => buyer.Id == (sale as Sale).BuyerId) as Buyer).Country
                != ((dataBase as DataBase).tables[3].Find(shop => shop.Id == (sale as Sale).ShopId) as Shop).Country).Cast<Sale>().ToList();
            }
            catch (Exception e)
            { throw new DataBaseException($"Неизвестная ошибка! \n{e.Message}"); }
        }

        // Получает общую сумму объема продаж
        long IDataAccessLayer.GetTotalSalesValue(IDataBase dataBase)
        {
            CheckTableExist(new List<int> { 1, 2 }, dataBase);

            try
            {
                return (dataBase as DataBase).tables[2].Sum(sale => (((dataBase as DataBase).tables[1].Find(good => good.Id == (sale as Sale).GoodId)) as Good).Price * (sale as Sale).GoodCount);
            }
            catch (Exception e)
            { throw new DataBaseException($"Неизвестная ошибка! \n{e.Message}"); }
        }

        // (Вспомогательный метод) Проверяет, что нужные таблицы существуют
        void CheckTableExist(List<int> nums, IDataBase dataBase)
        {
            foreach (int num in nums)
            {
                if ((dataBase as DataBase).tables[num] == null)
                {
                    throw new DataBaseException("Не все таблицы существуют!");
                }
            }
        }

    }
}
