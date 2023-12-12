using FurnitureFactory.Areas.FurnitureFactory.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FurnitureFactory.Areas.FurnitureFactory.TagHelpers;

public class PageLinkTagHelper : TagHelper
{
    private readonly IUrlHelperFactory _urlHelperFactory;

    public PageLinkTagHelper(IUrlHelperFactory helperFactory)
    {
        _urlHelperFactory = helperFactory;
    }

    [ViewContext] [HtmlAttributeNotBound] public ViewContext ViewContext { get; set; }

    public PageViewModel PageModel { get; set; }
    public string PageAction { get; set; }

    [HtmlAttributeName(DictionaryAttributePrefix = "page-url-")]
    public Dictionary<string, object> PageUrlValues { get; set; } = new();

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);
        output.TagName = "div";

        // набор ссылок будет представлять список ul
        var tag = new TagBuilder("ul");
        tag.AddCssClass("pagination");

        // формируем три ссылки - на текущую, предыдущую и следующую
        var currentItem = CreateTag(PageModel.PageNumber, urlHelper);

        // создаем ссылку на предыдущую страницу, если она есть
        if (PageModel.HasPreviousPage)
        {
            var prevItem = CreateTag(PageModel.PageNumber - 1, urlHelper);
            tag.InnerHtml.AppendHtml(prevItem);
        }

        tag.InnerHtml.AppendHtml(currentItem);
        // создаем ссылку на следующую страницу, если она есть
        if (PageModel.HasNextPage)
        {
            var nextItem = CreateTag(PageModel.PageNumber + 1, urlHelper);
            tag.InnerHtml.AppendHtml(nextItem);
        }

        output.Content.AppendHtml(tag);
    }

    private TagBuilder CreateTag(int pageNumber, IUrlHelper urlHelper)
    {
        var item = new TagBuilder("li");
        var link = new TagBuilder("a");
        if (pageNumber == PageModel.PageNumber)
        {
            item.AddCssClass("page-item active");
        }
        else
        {
            item.AddCssClass("page-item");
            PageUrlValues["page"] = pageNumber;
            link.Attributes["href"] = urlHelper.Action(PageAction, PageUrlValues);
        }
        link.AddCssClass("page-link");

        link.InnerHtml.Append(pageNumber.ToString());
        item.InnerHtml.AppendHtml(link);
        return item;
    }
}