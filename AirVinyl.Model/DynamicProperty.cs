using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace AirVinyl.Model
{
	public class DynamicProperty
	{
		[Key]
		[Column(Order = 1)]
		public string Key { get; set; }

		public string SpecializedValue { get; set; }

		public object Value
		{
			get { return  JsonConvert.DeserializeObject(SpecializedValue); }
			set { SpecializedValue = JsonConvert.SerializeObject(value); }
		}

		[Key]
		[Column(Order = 2)]
		public int VinylRecordId { get; set; }

		public virtual VinylRecord VinylRecord { get; set; }

		
	}
}