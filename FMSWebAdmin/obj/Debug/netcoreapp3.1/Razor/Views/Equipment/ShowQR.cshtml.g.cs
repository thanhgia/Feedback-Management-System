#pragma checksum "C:\Users\Admin\Desktop\New folder\FMSWebAdmin\FMSWebAdmin\Views\Equipment\ShowQR.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "5c3d78bcbaa41a875f6907f6a414d9f2ce198544"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Equipment_ShowQR), @"mvc.1.0.view", @"/Views/Equipment/ShowQR.cshtml")]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#nullable restore
#line 1 "C:\Users\Admin\Desktop\New folder\FMSWebAdmin\FMSWebAdmin\Views\_ViewImports.cshtml"
using FMSWebAdmin;

#line default
#line hidden
#nullable disable
#nullable restore
#line 2 "C:\Users\Admin\Desktop\New folder\FMSWebAdmin\FMSWebAdmin\Views\_ViewImports.cshtml"
using FMSWebAdmin.Models;

#line default
#line hidden
#nullable disable
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"5c3d78bcbaa41a875f6907f6a414d9f2ce198544", @"/Views/Equipment/ShowQR.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"c93c65a17245c64015f9deb53e62188600a2f8f3", @"/Views/_ViewImports.cshtml")]
    public class Views_Equipment_ShowQR : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<dynamic>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            WriteLiteral("\r\n");
#nullable restore
#line 2 "C:\Users\Admin\Desktop\New folder\FMSWebAdmin\FMSWebAdmin\Views\Equipment\ShowQR.cshtml"
  
    ViewData["Title"] = "ShowQR";
    Layout = "~/Views/Shared/_Layout.cshtml";

#line default
#line hidden
#nullable disable
            WriteLiteral("<script>\r\n        $(document).ready(function () {\r\n            getUrl();\r\n\r\n            function getUrl() {\r\n                var rawUrl = ");
#nullable restore
#line 11 "C:\Users\Admin\Desktop\New folder\FMSWebAdmin\FMSWebAdmin\Views\Equipment\ShowQR.cshtml"
                        Write(Html.Raw(ViewBag.Url));

#line default
#line hidden
#nullable disable
            WriteLiteral(@";
                var img = document.createElement('img');
                img.src = ""data:image/png;base64,"" + rawUrl.r;
                img.id = ""imgQR"";
                document.getElementById('qrCode').appendChild(img);
            }
        });
</script>
<style>
    #imgQR {
        width: 200px;
        height: 200px;
        margin-left: 40%;
        margin-top: 20px;
    }
</style>
<div class=""card"">
    <div id=""qrCode""></div>
</div>
");
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<dynamic> Html { get; private set; }
    }
}
#pragma warning restore 1591
