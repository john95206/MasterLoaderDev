using System;
using UnityEngine;

namespace MasterLoaderConfig
{
    [Serializable]
    public class Result : IResult
    {
        [SerializeField]
        private string _sheetUrl = string.Empty;
        [SerializeField]
        private string[] _masters = new string[] { };
        [SerializeField]
        private string[] _alerts = new string[] { };

        public string SheetUrl => _sheetUrl;
        public string[] Masters => _masters;
        public string[] Alerts => _alerts;

        public void SetMasters(string[] masters)
        {
            _masters = masters;
        }

        public void SetAlerts(string[] alerts)
        {
            _alerts = alerts;
        }

        public void SetSheetUrl(string sheetUrl)
        {
            _sheetUrl = sheetUrl;
        }
    }
}
