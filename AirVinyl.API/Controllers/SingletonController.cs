using System.Linq;
using System.Net;
using System.Web.Http;
using AirVinyl.API.Helpers;
using AirVinyl.DataAccessLayer;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;

namespace AirVinyl.API.Controllers
{
	public class SingletonController : ODataController
	{
		private AirVinylDbContext _ctx = new AirVinylDbContext();

		[HttpGet]
		[ODataRoute("Tim")]
		public IHttpActionResult GetSingletonTim()
		{
			var tim = _ctx.People.FirstOrDefault(p => p.PersonId == 6);

			return Ok(tim);
		}

		[HttpGet]
		[ODataRoute("Tim/Email")]
		[ODataRoute("Tim/FirstName")]
		[ODataRoute("Tim/LastName")]
		[ODataRoute("Tim/DateOfBirth")]
		[ODataRoute("Tim/Gender")]
		public IHttpActionResult GetPersonProperty()
		{
			var person = _ctx.People.FirstOrDefault(p => p.PersonId == 6);
			if (person == null)
			{
				return NotFound();
			}

			var propertyName = Url.Request.RequestUri.Segments.Last();
			if (!person.HasProperty(propertyName))
			{
				return NotFound();
			}

			var propValue = person.GetValue(propertyName);

			if (propValue == null)
			{
				return StatusCode(HttpStatusCode.NoContent);
			}

			return this.CreateOkHttpActionResult(propValue);
		}


		[HttpGet]
		[ODataRoute("Tim/Email/$value")]
		[ODataRoute("Tim/FirstName/$value")]
		[ODataRoute("Tim/LastName/$value")]
		[ODataRoute("Tim/DateOfBirth/$value")]
		[ODataRoute("Tim/Gender/$value")]
		public IHttpActionResult GetPersonPropertyRawValue()
		{
			var person = _ctx.People.FirstOrDefault(p => p.PersonId == 6);
			if (person == null)
			{
				return NotFound();
			}

			var propertyName = Url.Request.RequestUri
			                      .Segments[Url.Request.RequestUri.Segments.Length - 2].TrimEnd('/');
			if (!person.HasProperty(propertyName))
			{
				return NotFound();
			}

			var propValue = person.GetValue(propertyName);

			if (propValue == null)
			{
				return StatusCode(HttpStatusCode.NoContent);
			}

			return this.CreateOkHttpActionResult(propValue.ToString());
		}

		protected override void Dispose(bool disposing)
		{
			_ctx.Dispose();
			base.Dispose(disposing);
		}
	}
}