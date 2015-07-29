<%@ Assembly Name="Barista.SharePoint, Version=1.0.0.0, Culture=neutral, PublicKeyToken=a2d8064cb9226f52" %>
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
    <script type="text/javascript" src="/_admin/BaristaService/Scripts/Vendor/modernizr.custom.32669.js"></script>
    <SharePoint:CssLink Id="BootstrapCssLink" runat="server" DefaultUrl="/_admin/BaristaService/styles/bootstrap.min.css"></SharePoint:CssLink>
    <SharePoint:CssLink Id="AngularNgGridCssLink" runat="server" DefaultUrl="/_admin/BaristaService/Scripts/Vendor/ng-grid/ng-grid.css"></SharePoint:CssLink>
    <SharePoint:CssLink Id="ToastrCssLink" runat="server" DefaultUrl="/_admin/BaristaService/Scripts/Vendor/toastr/toastr.css"></SharePoint:CssLink>
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

        .toolbar {
            padding-top: 5px;
            padding-bottom: 10px;
        }

        .gridStyle {
            border: 1px solid rgb(212,212,212);
            height: 300px;
        }

        /* Override Bootstrap styles that conflict with corev4.css*/
        img {
            max-width: none;
        }
		
		.nav.nav-tabs {
			height: 37px;
		}
    </style>
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <SharePoint:FormDigest ID="FormDigest1" runat="server" />
    <div style="padding:5px;" data-ng-app="managebarista" data-ng-controller="ManageMainCtrl">
        <div data-tabset="">
            <div data-tab="" data-heading="General" style="cursor: pointer;">
				<div data-ng-include="'/_admin/BaristaService/Views/General.html'" data-ng-controller="ManageGeneralCtrl"></div>
            </div>
			<div data-tab="" data-heading="Indexes" style="cursor: pointer;">
				<div data-ng-include="'/_admin/BaristaService/Views/Indexes.html'" data-ng-controller="ManageIndexesCtrl"></div>
            </div>
            <div data-tab="" data-heading="Packages" style="cursor: pointer;">
				<div data-ng-include="'/_admin/BaristaService/Views/Packages.html'" data-ng-controller="ManagePackagesCtrl"></div>
            </div>
        </div>
    </div>
    
    <script type="text/javascript" src="/_admin/BaristaService/Scripts/Vendor/json3.min.js"></script>
    
    <script type="text/javascript" src="/_admin/BaristaService/Scripts/Vendor/jquery/jquery-1.10.2.min.js"></script>
    <script type="text/javascript" src="/_admin/BaristaService/Scripts/Vendor/lodash.js"></script>
    <script type="text/javascript" src="/_admin/BaristaService/Scripts/Vendor/toastr/toastr.js"></script>
    <script type="text/javascript" src="/_admin/BaristaService/Scripts/Vendor/angular/angular.js"></script>

    <script type="text/javascript" src="/_admin/BaristaService/Scripts/Vendor/ui-bootstrap/ui-bootstrap-tpls-0.11.0.min.js"></script>
    <script type="text/javascript" src="/_admin/BaristaService/Scripts/Vendor/ng-grid/ng-grid-2.0.11.min.js"></script>
    
    <script type="text/javascript" src="/_admin/BaristaService/Scripts/app.js?v=20150724"></script>
    <script type="text/javascript" src="/_admin/BaristaService/Scripts/Controllers/ManageGeneralCtrl.js?v=20150724"></script>
    <script type="text/javascript" src="/_admin/BaristaService/Scripts/Controllers/ManageIndexesCtrl.js?v=20150724"></script>
    <script type="text/javascript" src="/_admin/BaristaService/Scripts/Controllers/ManagePackagesCtrl.js?v=20150724"></script>
    
    <script type="text/javascript">
        $(document).ready(function () {
            //Kill the search area, it just looks bad (style conflict?)
            $("#s4-searcharea").hide();
        });
    </script>
</asp:Content>