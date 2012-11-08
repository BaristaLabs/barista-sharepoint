<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Manage.aspx.cs" Inherits="Barista.SharePoint.Manage" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
    Barista Service Application
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
    Manage Barista Service Application
</asp:Content>

<asp:Content ID="PlaceHolderAdditionalPageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
    <SharePoint:CssLink Id="BootstrapCssLink" runat="server" DefaultUrl="/_layouts/BaristaJS/bootstrap-2.1.1/css/bootstrap.min.css"></SharePoint:CssLink>
    <SharePoint:CssLink Id="KendoCommonCssLink" runat="server" DefaultUrl="/_layouts/BaristaJS/kendoui.web.2012.2.913/styles/kendo.common.min.css"></SharePoint:CssLink>
    <SharePoint:CssLink Id="KendoDefaultCssLink" runat="server" DefaultUrl="/_layouts/BaristaJS/kendoui.web.2012.2.913/styles/kendo.default.min.css"></SharePoint:CssLink>
    <style type="text/css">
        a:visited {
            color: black;
        }

        div.k-window-content {
            position: static;
        }

        div.k-window {
            display: table;
        }
    </style>
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
	<table border="0" cellspacing="0" cellpadding="0" width="90%" style="margin: 5px;">
		<tr>
			<td style="padding-top: 10px">
                <b>Trusted Barista Locations:</b>
				<div id="trustedLocationsGrid"></div>	
			</td>
		</tr>
	</table>
    <script type="text/x-kendo-template" id="trustedLocationTemplate">
        <div class="editor form-horizontal">
            <div class="control-group">
                <label class="control-label" for="inputUrl">Url </label>
                <div class="controls">
                    <input id="inputUrl" type="text" placeholder="SharePoint Web Url" 
                        required data-required-msg="You need to enter a URL" data-url-msg="This url is invalid"
                        pattern="http(s?)\://.*"
                        data-bind="value:Url"></input>
                </div>
            </div>
            <div class="control-group">
                <label class="control-label" for="inputDescription">Description </label>
                <div class="controls">
                    <textarea id="inputDescription" rows="3" placeholder="Description"
                        data-bind="value:Description">
                    </textarea>
                </div>
            </div>
            <div class="control-group">
                <label class="control-label" for="inputLocationType">Location Type </label>
                <div class="controls">
                    <select id="inputLocationType" data-bind="value:LocationType">
                        <option value="Web">Web</option>
                    </select>
                </div>
            </div>
            <div class="control-group">
                <label class="control-label" for="inputTrustChildren">Trust Children? </label>
                <div class="controls">
                    <input id="inputTrustChildren" type="checkbox"
                        data-bind="checked:TrustChildren"></input>
                </div>
            </div>
        </div>
    </script>
    <script type="text/x-kendo-template" id="toolbarTemplate">
        <div class="toolbar">
            <div class="btn-group">
                <a id="btnNewLocation" class="btn btn-primary" href="\#">
                    <img src="/_layouts/images/newitem.gif" alt="New" /><span style="color: white;"> New Location</span>
                </a>
            </div>
        </div>
    </script>
    <script type="text/javascript" src="/_layouts/BaristaJS/json2.js"></script>
    <script type="text/javascript" src="/_layouts/BaristaJS/modernizr-2.6.2.js"></script>
    <script type="text/javascript" src="/_layouts/BaristaJS/jquery-1.8.2.min.js"></script>
    <script type="text/javascript" src="/_layouts/BaristaJS/jquery.validate-1.10.0.min.js"></script>
    <script type="text/javascript" src="/_layouts/BaristaJS/jquery.validate.additional-methods-1.10.0.min.js"></script>
    <script type="text/javascript" src="/_layouts/BaristaJS/bootstrap-2.1.1/js/bootstrap.min.js"></script>
    <script type="text/javascript" src="/_layouts/BaristaJS/kendoui.web.2012.2.913/js/kendo.web.min.js"></script>
    <script type="text/javascript">
        var trustedLocationsDataSource;
        $(document).ready(function () {
            //Kill the search area, it just looks bad (style conflict?)
            $("#s4-searcharea").hide();

            var trustedLocationsServiceBaseUrl = "/_vti_bin/Barista/v1/Barista.svc/eval?c=";
            var trustedLocationsServiceBasePostUrl = "/_vti_bin/Barista/v1/Barista.svc/eval?c=";
            trustedLocationsDataSource = new kendo.data.DataSource({
                transport: {
                    read: {
                        url: trustedLocationsServiceBaseUrl + encodeURIComponent("/_admin/BaristaService/GetTrustedLocations.js"),
                        dataType: "json",
                        type: "GET"
                    },
                    update: {
                        url: trustedLocationsServiceBasePostUrl + encodeURIComponent("/_admin/BaristaService/AddOrUpdateTrustedLocation.js"),
                        contentType: "application/x-www-form-jsonencoded",
                        dataType: "json",
                        type: "POST"
                    },
                    destroy: {
                        url: trustedLocationsServiceBasePostUrl + encodeURIComponent("/_admin/BaristaService/DeleteTrustedLocation.js"),
                        contentType: "application/x-www-form-jsonencoded",
                        dataType: "json",
                        type: "POST",
                    },
                    create: {
                        url: trustedLocationsServiceBasePostUrl + encodeURIComponent("/_admin/BaristaService/AddOrUpdateTrustedLocation.js"),
                        contentType: "application/x-www-form-jsonencoded",
                        dataType: "json",
                        type: "POST",
                    },
                    parameterMap: function (options, operation) {
                        if (operation !== "read" && options.models) {
                            return kendo.stringify(options.models[0]);
                        }
                    }
                },
                batch: true,
                pageSize: 15,
                schema: {
                    model: {
                        id: "Url",
                        fields: {
                            Url: { type: "url", required: true },
                            Description: { type: "string" },
                            LocationType: { type: "string", defaultValue: "Web" },
                            TrustChildren: { type: "boolean" }
                        }
                    }
                }
            });

            $("#trustedLocationsGrid").kendoGrid({
                dataSource: trustedLocationsDataSource,
                height: 350,
                scrollable: true,
                sortable: true,
                filterable: true,
                pageable: {
                    input: true,
                    numeric: false
                },
                toolbar: $("#toolbarTemplate").html(),
                columns: [
                    {
                        field: "Url",
                        title: "Url",
                        width: 400
                    },
                    {
                        field: "Description",
                        title: "Description",
                        width: 200
                    },
                    {
                        field: "LocationType",
                        title: "Location Type",
                        width: 100
                    },
                    {
                        field: "TrustChildren",
                        title: "Trust Children",
                        width: 100
                    },
                    { command: ["edit", "destroy"], title: "&nbsp;", width: "250px" }
                ],
                editable: {
                    mode: "popup",
                    template: $("#trustedLocationTemplate").html(),
                    }
            });

            $("#btnNewLocation").click(function (event) {
                event.stopPropagation();
                var grid = $("#trustedLocationsGrid").data("kendoGrid");
                grid.addRow();
            });
        });
    </script>
</asp:Content>