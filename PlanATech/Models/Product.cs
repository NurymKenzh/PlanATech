using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlanATech.Models
{
    /// <summary>
    /// Products
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Unique Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Category Id
        /// </summary>
        public int CategoryId { get; set; }
        /// <summary>
        /// Category of the product
        /// </summary>
        public Category Category { get; set; }
        /// <summary>
        /// Name of the product
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Description of the product
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Product specification data
        /// </summary>
        public string Data { get; set; }
    }
}
