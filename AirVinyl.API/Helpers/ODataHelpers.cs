using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.OData;

namespace AirVinyl.API.Helpers
{
    public static  class ODataHelpers
    {
        public static bool HasProperty(this object instance, string propertyName)
        {
            var pi = instance.GetType().GetProperty(propertyName);
            return pi != null;
        }

        public static object GetValue(this object instance, string propertyName)
        {
            var pi = instance.GetType().GetProperty(propertyName);
            if(pi == null)
                throw new HttpException($"Can't find property" +
                                        $": {propertyName}");

            var propValue = pi.GetValue(instance, new object[] { });
            return propValue;
        }

        public static IHttpActionResult CreateOkHttpActionResult(this ODataController controller, object value)
        {
            var okMethod = default(MethodInfo);
            var methods = controller.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);

            okMethod = methods.FirstOrDefault(m => m.Name == "Ok" && m.GetParameters().Length == 1);
            okMethod = okMethod.MakeGenericMethod(value.GetType());
            var returnValue = okMethod.Invoke(controller, new object[] {value});
            return (IHttpActionResult) returnValue;
        }

        public static int GetIntKey(this Uri url)
        {
	        var pattern = "\\d+";
	        var last = url.Segments.Last();
	        var result = Regex.Match(last, pattern);

	        return result.Success ? int.Parse(result.Value) : -1;
        }
        
        
    }
}