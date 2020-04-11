using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AirVinyl.Model
{
    public class VinylRecord
    {
        [Key]
        public int VinylRecordId { get; set; }

        [StringLength(150)]
        [Required]
        public string Title { get; set; }

        [StringLength(150)]
        [Required]
        public string Artist { get; set; }

        [StringLength(50)]
        public string CatalogNumber { get; set; }

        public int? Year { get; set; }

        public PressingDetail PressingDetail { get; set; }

        public virtual Person Person { get; set; }

        private IDictionary<string, object> _properties;

        public IDictionary<string,object> Properties
        {
	        get
	        {
                if(_properties == null)
                {
                    _properties = new Dictionary<string, object>();
                    foreach (var dynamicProperty in DynamicProperties)
                    {
	                    _properties.Add(dynamicProperty.Key, dynamicProperty.Value);
                    }
                }

                return _properties;
	        }
            set { _properties = value; }
        }

        public ICollection<DynamicProperty> DynamicProperties { get; set; }

        public VinylRecord()
        {
	        DynamicProperties = new List<DynamicProperty>();
        }
        
    }
}
