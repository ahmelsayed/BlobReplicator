using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace BlobStorageReplicator.Code
{
    public static class HtmlExtensions
    {
        public static MvcHtmlString BootstrapTextBoxFor<TModel, TValue>(this HtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression)
        {
            return html.TextBoxFor(expression, new { @class = "form-control" });
        }

        public static MvcHtmlString BootstrapLabelFor<TModel, TValue>(this HtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression)
        {
            return html.LabelFor(expression, new { @class = "control-label col-xs-2" });
        }

        public static MvcForm BootstrapBeginForm(this HtmlHelper html, string actionName, string controllerName, FormMethod method)
        {
            return html.BeginForm(actionName, controllerName, method, new {@class = "form-horizontal"});
        }
    }
}