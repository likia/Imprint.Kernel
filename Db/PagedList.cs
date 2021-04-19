using System;
using System.Collections.Generic;
using System.Linq;

namespace Imprint.Db
{
    /// <summary>
    /// ��ҳ���ݼ��ϣ����ں�˷��ط�ҳ�õļ��ϼ�ǰ����ͼ��ҳ�ؼ���
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PagedList<T> : List<T>
    {
        /// <summary>
        /// �ڴ��ҳ,��ȫ�����ݶ�ȡ�����ڴ��ж�̬��ҳ.��������
        /// </summary>
        /// <param name="items">δ��ҳ��ȫ������</param>
        /// <param name="pageIndex">��ǰҳ</param>
        /// <param name="pageSize">��ҳ��</param>
        public PagedList(IList<T> items, int pageIndex, int pageSize)
        {
            PageSize = pageSize;
            TotalItemCount = items.Count;
            CurrentPageIndex = pageIndex;
            for (int i = StartRecordIndex - 1; i < EndRecordIndex; i++)
            {
                Add(items[i]);
            }
        }

        /// <summary>
        /// ���ݿ��ҳ,ͨ�������Ѿ���ҳ�õ�����ֱ�ӷ���
        /// </summary>
        /// <param name="items">��XXҳ�������</param>
        /// <param name="pageIndex">��ǰҳ</param>
        /// <param name="pageSize">��ҳ��</param>
        /// <param name="totalItemCount"></param>
        public PagedList(IEnumerable<T> items, int pageIndex, int pageSize, int totalItemCount)
        {
            AddRange(items);
            TotalItemCount = totalItemCount;
            CurrentPageIndex = pageIndex;
            PageSize = pageSize;
        }

        public int ExtraCount { get; set; }
        public int CurrentPageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalItemCount { get; set; }
        public int TotalPageCount { get { return (int)Math.Ceiling(TotalItemCount / (double)PageSize); } }
        public int StartRecordIndex { get { return (CurrentPageIndex - 1) * PageSize + 1; } }
        public int EndRecordIndex { get { return TotalItemCount > CurrentPageIndex * PageSize ? CurrentPageIndex * PageSize : TotalItemCount; } }
    }

    /// <summary>
    /// Linq��ҳ��չ����
    /// </summary>
    public static class PageLinqExtensions
    {
        public static PagedList<T> ToPagedList<T>
            (
                this IQueryable<T> allItems,
                int pageIndex,
                int pageSize
            )
        {
            if (pageIndex < 1)
                pageIndex = 1;
            var itemIndex = (pageIndex - 1) * pageSize;
            var pageOfItems = allItems.Skip(itemIndex).Take(pageSize).ToList();
            var totalItemCount = allItems.Count();
            return new PagedList<T>(pageOfItems, pageIndex, pageSize, totalItemCount);
        }
    }
}