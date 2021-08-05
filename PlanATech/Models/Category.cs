using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlanATech.Models
{
    /// <summary>
    /// Category of product
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Unique Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Name of the category
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Description of the category
        /// </summary>
        public string Description { get; set; }
    }
}
