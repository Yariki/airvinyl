using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Services.Protocols;
using AirVinyl.Model;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;

namespace AirVinyl.API
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            //config.MapHttpAttributeRoutes();

            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);

            config.MapODataServiceRoute("ODataRoute", "odata", GetEdmModel());
            config.Count().Filter().OrderBy().Expand().Select().MaxTop(null);
            config.EnsureInitialized();
        }

        private static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.Namespace = "AirVinyl";
            builder.ContainerName = "AirVinylContainer";

            builder.EntitySet<Person>("People");
            //builder.EntitySet<VinylRecord>("VinylRecords");
            builder.EntitySet<RecordStore>("RecordStores");

            var isHighRatedFunction = builder.EntityType<RecordStore>().Function("IsHighRated");
            isHighRatedFunction.Returns<bool>();
            isHighRatedFunction.Parameter<int>("minimumRating");
            isHighRatedFunction.Namespace = "AirVinyl.Functions";

            var areRatedByFunction = builder.EntityType<RecordStore>()
                                            .Collection.Function("AreRatedBy");

            areRatedByFunction.ReturnsCollectionFromEntitySet<RecordStore>("RecordStores");
            areRatedByFunction.CollectionParameter<int>("personIds");
            areRatedByFunction.Namespace = "AirVinyl.Functions";

            var getHighRatedRecordStoresFunctions = builder.Function("GetHighRatedRecordStores");
            getHighRatedRecordStoresFunctions.Parameter<int>("minimumRating");
            getHighRatedRecordStoresFunctions.ReturnsCollectionFromEntitySet<RecordStore>("RecordStores");
            getHighRatedRecordStoresFunctions.Namespace = "AirVinyl.Functions";

            var rateAction = builder.EntityType<RecordStore>().Action("Rate");
            rateAction.Returns<bool>();
            rateAction.Parameter<int>("rating");
            rateAction.Parameter<int>("personId");
            rateAction.Namespace = "AirVinyl.Actions";

            builder.Singleton<Person>("Tim");

            return builder.GetEdmModel();

        }

    }
}
