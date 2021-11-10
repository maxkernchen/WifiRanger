
using System.Windows.Media.Imaging;

namespace WifiRanger
{
    /// <summary>
    /// Class which holds getter and setter methods for Router Data
    /// <author>Max Kernchen</author>
    /// <date>11/09/2021</date>
    /// </summary>
    public class RouterData
    {
        // the brand name of the router
        private string _brand;
        /// <summary>
        /// gets or sets the brand name of the router
        /// </summary>
        public string Brand
        {
            get { return this._brand; }
            set { this._brand = value; }
        }
        // the image of the router
        private BitmapImage _imageData;
        /// <summary>
        /// gets or sets the image of the router
        /// </summary>
        public BitmapImage ImageData
        {
            get { return this._imageData; }
            set { this._imageData = value; }
        }
        // the model of the router
        private string _model;
        /// <summary>
        /// gets or sets the model of the router
        /// </summary>
        public string Model
        {
            get { return this._model; }
            set { this._model = value; }
        }
        //the price of the router
        private string _price;
        /// <summary>
        /// gets or sets the price of the router
        /// </summary>
        public string Price
        {
            get { return this._price; }
            set { this._price = value; }
        }
        //the URL of the router's store page
        private string _url;
        /// <summary>
        /// Gets or sets the URL
        /// </summary>
        public string URL
        {
            get { return this._url; }
            set { this._url = value; }
        }

        //the review ratings of the router's store page
        private string _rating;
        /// <summary>
        /// Gets or sets the URL
        /// </summary>
        public string Rating
        {
            get { return this._rating; }
            set { this._rating = value; }
        }
        // area used for tracking validation of area inputs for each router
        private int _area;
        /// <summary>
        /// gets or sets the area that the router is supposed to cover
        /// </summary>
        public int Area
        {
            get { return this._area; }
            set { this._area = value; }
        }
        //the the upc of the item for unique id
        private string _itemID;
        /// <summary>
        /// Gets or sets the ItemID
        /// </summary>
        public string ItemID
        {
            get { return this._itemID; }
            set { this._itemID = value; }
        }

        // URL name we use for the hyperlink, not actual url. 
        private string _urlName;
        /// <summary>
        /// Gets or sets the URLName
        /// </summary>
        public string URLName
        {
            get { return this._urlName; }
            set { this._urlName = value; }
        }

        // frequency for router used for coverage calculation
        private string _frequency;
        /// <summary>
        /// Gets or sets the Frequency
        /// </summary>
        public string Frequency
        {
            get { return this._frequency; }
            set { this._frequency = value; }
        }


        // Power for router used for coverage calculation
        private string _power;
        /// <summary>
        /// Gets or sets the Power
        /// </summary>
        public string Power
        {
            get { return this._power; }
            set { this._power = value; }
        }


        // isHighPower bool for router used for coverage calculation
        private bool _isHighPower;
        /// <summary>
        /// Gets or sets isHighPower
        /// </summary>
        public bool IsHighPower
        {
            get { return this._isHighPower; }
            set { this._isHighPower = value; }
        }


    }
}

