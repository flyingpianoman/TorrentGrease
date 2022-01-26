using SpecificationTest.Pages.Components;

namespace SpecificationTest.Pages
{
    internal interface IPageWithMinFileSize : IPage
    {
        int MinFileSize { get; set; }
        RadioComponent MinFileSizeUnit { get; }
    }
}
