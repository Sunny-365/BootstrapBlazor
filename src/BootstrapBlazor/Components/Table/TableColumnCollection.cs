﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BootstrapBlazor.Components
{
    /// <summary>
    /// Table Header 组件
    /// </summary>
    public class TableColumnCollection : ComponentBase
    {
        /// <summary>
        /// Specifies the content to be rendered inside this
        /// </summary>
        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// 获得/设置 排序回调方法
        /// </summary>
        [Parameter]
        public Func<string, SortOrder, Task>? OnSortAsync { get; set; }

        /// <summary>
        /// 获得/设置 过滤回调方法
        /// </summary>
        [Parameter]
        public Func<IEnumerable<FilterKeyValueAction>, Task>? OnFilterAsync { get; set; }

        /// <summary>
        /// 获得 表头集合
        /// </summary>
        internal ICollection<ITableColumn> Columns { get; } = new List<ITableColumn>(50);

        private List<Action> Notifications { get; } = new List<Action>(20);

        /// <summary>
        /// BuildRenderTree 方法
        /// </summary>
        /// <param name="builder"></param>
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var index = 0;
            builder.OpenComponent<CascadingValue<TableColumnCollection>>(index++);
            builder.AddAttribute(index++, "Value", this);
            builder.AddAttribute(index++, "IsFixed", true);
            builder.AddAttribute(index++, "ChildContent", ChildContent);
            builder.CloseComponent();
        }

        /// <summary>
        /// 过滤弹窗集合
        /// </summary>
        private Dictionary<string, TableFilterBase> Filters { get; set; } = new Dictionary<string, TableFilterBase>();

        /// <summary>
        /// 过滤条件集合
        /// </summary>
        private Dictionary<string, IEnumerable<FilterKeyValueAction>> FilterActions { get; set; } = new Dictionary<string, IEnumerable<FilterKeyValueAction>>();

        /// <summary>
        /// 注册过滤条件变化通知
        /// </summary>
        /// <param name="callback"></param>
        internal void RegisterFilterChangedNotify(Action callback)
        {
            Notifications.Add(callback);
        }

        /// <summary>
        /// 增加 Filter 方法
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="filter"></param>
        internal void AddFilter(string fieldName, TableFilterBase filter)
        {
            Filters.Add(fieldName, filter);
        }

        /// <summary>
        /// 判断是否有过滤存在
        /// </summary>
        /// <param name="filterKey"></param>
        /// <returns></returns>
        internal bool HasFilter(string filterKey)
        {
            return FilterActions.ContainsKey(filterKey);
        }

        /// <summary>
        /// 重置指定 Key 的过滤条件
        /// </summary>
        /// <param name="filterKey"></param>
        internal bool ResetFilter(string filterKey)
        {
            var ret = FilterActions.Remove(filterKey);
            if (ret) Notifications.ForEach(c => c.Invoke());
            return ret;
        }

        /// <summary>
        /// 添加指定 Key 的过滤条件
        /// </summary>
        /// <param name="filterKey"></param>
        /// <param name="filters"></param>
        internal void AddFilters(string filterKey, IEnumerable<FilterKeyValueAction> filters)
        {
            // 过滤条件变化通知 Header 更新 UI
            FilterActions.Remove(filterKey);
            if (filters.Any()) FilterActions.Add(filterKey, filters);
            Notifications.ForEach(c => c.Invoke());
        }

        internal IEnumerable<FilterKeyValueAction> GetFilters() => FilterActions
            .Aggregate(new List<FilterKeyValueAction>(), (result, source) =>
            {
                result.AddRange(source.Value);
                return result;
            });

        /// <summary>
        /// 弹出 Filter 窗口方法
        /// </summary>
        /// <param name="fieldName"></param>
        internal void ShowFilter(string fieldName)
        {
            if (Filters.TryGetValue(fieldName, out var filter))
            {
                filter.ShowFilter();
            }
        }
    }
}
