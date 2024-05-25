using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _20231222_plateDetect
{
    public class Car_Plateinfo
    {
        public string id {  get; set; }
        public string project { get; set; }
        public string iteration { get; set; }
        public DateTime created { get; set; }
        public List<Prediction> predictions { get; set; }
    }
    public class Prediction
    {
        public double probability { get; set; }
        public string tagId { get; set; }
        public string tagname { get; set; }

        public BoundingBox BoundingBox { get; set; }
    }

    public class BoundingBox
    {
        public double left{get; set;}
        public double top { get; set; }
        public double width { get; set; }
        public double height { get; set; }

    }

}
