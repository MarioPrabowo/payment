using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Presentation.PaymentApi
{
	/// <summary>
	/// <see cref="ApiControllerAttribute"/> provides convenient features likes model state validation;
	/// See https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-2.1#annotate-class-with-apicontrollerattribute
	///
	/// <see cref="IRouteTemplateProvider"/> allows us to not define <see cref="RouteAttribute"/> on every controller
	/// See https://github.com/aspnet/AspNetCore.Docs/blob/master/aspnetcore/mvc/controllers/routing.md#custom-route-attributes-using-iroutetemplateprovider
	/// </summary>
	public class StandardApiControllerAttribute :
		ApiControllerAttribute,
		IRouteTemplateProvider
	{
		string IRouteTemplateProvider.Template => "[controller]";

		int? IRouteTemplateProvider.Order => null;

		// Each route template must have a unique name
		string IRouteTemplateProvider.Name => System.Guid.NewGuid().ToString();
	}
}