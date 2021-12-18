/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================


namespace Blazr.UI
{
    public partial class UIListColumn : UIComponentBase
    {
        [CascadingParameter(Name = "IsHeader")] public bool IsHeader { get; set; }
        [Parameter] public bool IsMaxColumn { get; set; }
        [Parameter] public string HeaderTitle { get; set; } = string.Empty;
        [Parameter] public bool IsHeaderNoWrap { get; set; }
        [Parameter] public bool NoWrap { get; set; }

        private bool isMaxRowColumn => IsMaxColumn && !this.IsHeader;
        private bool isNormalRowColumn => !IsMaxColumn && !this.IsHeader;
        protected override List<string> UnwantedAttributes { get; set; } = new List<string>() { "class" };

        private string HeaderCss
            => CSSBuilder.Class()
                .AddClass(IsHeaderNoWrap,"header-column-nowrap", "header-column")
                .AddClass("text-nowrap", NoWrap)
                .AddClass("align-baseline")
                .Build();
        private string TDCss
            => CSSBuilder.Class()
                .AddClass(this.isMaxRowColumn,"max-column", "data-column")
                .AddClass("text-nowrap", this.NoWrap)
                .Build();
    }
}
