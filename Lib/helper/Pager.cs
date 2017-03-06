﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lib.helper
{

    /// <summary>
    /// 分页用的数据模型，请不要返回空对象
    /// </summary>
    public class PagerData<T>
    {
        public readonly Dictionary<string, string> UrlParams = new Dictionary<string, string>();

        /// <summary>
        /// 数据库记录总数
        /// </summary>
        public int ItemCount { get; set; }

        /// <summary>
        /// 总页数，通过itemcount和pagesize计算出
        /// </summary>
        public int PageCount
        {
            get
            {
                if (this.PageSize < 1) { return 0; }
                return PagerHelper.GetPageCount(this.ItemCount, this.PageSize);
            }
        }

        /// <summary>
        /// 每页显示数量
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 当前页码
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// 查出来的数据列表
        /// </summary>
        public List<T> DataList { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// 分页控件的html代码
        /// </summary>
        public string GetPagerHtml(string base_url, string pageKey, int currentPage, int pageSize)
        {
            return PagerHelper.GetPagerHtmlByData(
                url: base_url,
                pageKey: pageKey,
                urlParams: UrlParams,
                itemCount: ItemCount,
                page: currentPage,
                pageSize: pageSize);
        }
    }

    /// <summary>
    /// 分页帮助类
    /// </summary>
    public static class PagerHelper
    {
        /// <summary>
        /// 通过给定参数生成分页html
        /// </summary>
        /// <param name="url"></param>
        /// <param name="pageKey"></param>
        /// <param name="urlParams"></param>
        /// <param name="itemCount"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static string GetPagerHtmlByData(
            string url, string pageKey,
            Dictionary<string, string> urlParams,
            int itemCount, int page, int pageSize)
        {
            url = ConvertHelper.GetString(url).Trim();
            if (!url.EndsWith("?"))
            {
                url += "?";
            }

            if (!ValidateHelper.IsPlumpString(pageKey))
            {
                pageKey = "page";
            }

            url += Com.DictToUrlParams(urlParams);
            if (ValidateHelper.IsPlumpDict(urlParams))
            {
                url += "&";
            }

            url += pageKey + "={0}";

            int pageCount = GetPageCount(itemCount, pageSize);

            return GetPagerHtml(page, pageCount, url);
        }

        /// <summary>
        /// 
        /// 思路如下：
        /// 
        /// 1.生成第一页
        /// 2.计算游标开始位置
        /// 3.判断游标开始位置之前是否还有内容，如果有就添加省略号
        /// 4.计算游标结束位置
        /// 5.通过for循环生成游标开始结束的html
        /// 6.判断游标结束位置之后是否还有内容，如果有就添加省略号
        /// 7.生成最后一页
        /// 8.判断是否需要显示总页数
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageCount"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetPagerHtml(int page, int pageCount, string url)
        {
            //不需要分页
            if (pageCount <= 1) { return string.Empty; }
            //判断数据是否有用
            if (page < 1) { page = 1; }
            if (page > pageCount) { page = pageCount; }
            //判断参数是否正确
            if (url.IndexOf("{0}") < 0) { url += "{0}"; }

            var html = new StringBuilder();
            html.Append("<ul class='pagination'>");
            /////////////////////////////前面是判断数据是否有用，下面开始生成////////////////////////

            //第一页
            html.Append(string.Format("<li class='first-li{0}'><a href='{1}' data-page='{2}' target='_self'>1</a></li>",
                page == 1 ? " active" : string.Empty,
                string.Format(url, 1),
                1));
            //计算游标开始,结束的位置（除了第一页和最后一页其他数字就是游标的范围）
            int cursorStart = page - 3, cursorEnd = page + 3;
            //游标开始<=2
            if (cursorStart <= 2)
            {
                cursorStart = 2;
            }
            else
            {
                //如果游标开始位置大于2则表明在此游标前还有内容，加省略号。
                html.Append(string.Format("<li><a href='{0}' data-page='{1}' target='_self'>{2}</a></li>",
                    string.Format(url, cursorStart - 1),
                    cursorStart - 1,
                    "«"));
            }
            //判断游标结束位置是否还有更多内容
            bool hasMore = cursorEnd < pageCount - 1;
            //如果没有更多内容，游标结束就在最后一页前一页
            if (!hasMore)
            {
                cursorEnd = pageCount - 1;
            }
            //通过游标生成页码
            for (int i = cursorStart; i <= cursorEnd; ++i)
            {
                html.Append(string.Format("<li{0}><a href='{1}' data-page='{2}' target='_self'>{3}</a></li>",
                    page == i ? " class='active'" : string.Empty,
                    string.Format(url, i),
                    i,
                    i));
            }
            //如果有更多就加省略号
            if (hasMore)
            {
                html.Append(string.Format("<li><a href='{0}' data-page='{1}' target='_self'>{2}</a></li>",
                    string.Format(url, cursorEnd + 1),
                    cursorEnd + 1,
                    "»"));
            }
            //生成最后一页
            html.Append(string.Format("<li class='last-li{0}'><a href='{1}' data-page='{2}' target='_self'>{3}</a></li>",
                page == pageCount ? " active" : string.Empty,
                string.Format(url, pageCount),
                pageCount,
                pageCount));

            html.Append("</ul>");
            return html.ToString();
        }

        /// <summary>
        /// 计算总页数
        /// </summary>
        /// <param name="item_count"></param>
        /// <param name="page_size"></param>
        /// <returns></returns>
        public static int GetPageCount(int item_count, int page_size)
        {
            if (item_count <= 0) { return 0; }
            if (page_size < 1) { throw new Exception("pagesize不能小于1"); }
            return item_count % page_size == 0 ? (item_count / page_size) : (item_count / page_size + 1);
        }

        /// <summary>
        /// 计算mysql 的limit参数
        /// </summary>
        /// <param name="current_page"></param>
        /// <param name="page_size"></param>
        /// <returns></returns>
        public static int[] GetQueryRange(int current_page, int page_size)
        {
            if (current_page < 1) { throw new Exception("页码不能小于1"); }
            if (page_size < 1) { throw new Exception("pagesize不能小于1"); }
            return new int[] { ((current_page - 1) * page_size), page_size };
        }
    }

}