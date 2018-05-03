
using System.Windows.Media.Imaging;

namespace WifiRanger
{
    /// <summary>
    /// Class which holds getter and setter methods for Router Data
    /// </summary>
    class RouterData
    {
        // the brand name of the router
        private string _Brand;
        /// <summary>
        /// gets or sets the brand name of the router
        /// </summary>
        public string Brand
        {
            get { return this._Brand; }
            set { this._Brand = value; }
        }
        // the image of the router
        private BitmapImage _ImageData;
        /// <summary>
        /// gets or sets the image of the router
        /// </summary>
        public BitmapImage ImageData
        {
            get { return this._ImageData; }
            set { this._ImageData = value; }
        }
        // the model of the router
        private string _Model;
        /// <summary>
        /// gets or sets the model of the router
        /// </summary>
        public string Model
        {
            get { return this._Model; }
            set { this._Model = value; }
        }
        //the price of the router
        private string _Price;
        /// <summary>
        /// gets or sets the price of the router
        /// </summary>
        public string Price
        {
            get { return this._Price; }
            set { this._Price = value; }
        }
        //the URL of the router's store page
        private string _URL;
        /// <summary>
        /// Gets or sets the URL
        /// </summary>
        public string URL
        {
            get { return this._URL; }
            set { this._URL = value; }
        }
        // area used for tracking validation of area inputs for each router
        private int _Area;
        /// <summary>
        /// gets or sets the area that the router is supposed to cover
        /// </summary>
        public int Area
        {
            get { return this._Area; }
            set { this._Area = value; }
        }

    }
}

