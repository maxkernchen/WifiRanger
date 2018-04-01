﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace WifiRanger
{
    class RouterData
    {
        private string _Name;

        public string Name
        {
            get { return this._Name; }
            set { this._Name = value; }
        }

        private BitmapImage _ImageData;

        public BitmapImage ImageData
        {
            get { return this._ImageData; }
            set { this._ImageData = value; }
        }

        private string _Model;

        public string Model
        {
            get { return this._Model; }
            set { this._Model = value; }
        }
        private string _Price;

        public string Price
        {
            get { return this._Price; }
            set { this._Price = value; }
        }

        private string _URL;

        public string URL
        {
            get { return this._URL; }
            set { this._URL = value; }
        }

        private int _Area;

        public int Area
        {
            get { return this._Area; }
            set { this._Area = value; }
        }

    }
}

