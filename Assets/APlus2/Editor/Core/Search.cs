//  Copyright (c) 2020-present amlovey
//  
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace APlus2
{
    public class SearchCondition
    {
        public string FieldKey;
        public string[] Values;

        public bool Execute(APAsset asset)
        {
            var value = GetFiledValue(asset);
            
            bool isString = value is string | value is Enum;
            bool isNumber = value is int | value is long | value is double | value is short
                            | value is decimal | value is Single | value is ushort
                            | value is float | value is uint | value is ulong;
            bool isBoolean = value is bool | value is bool?;

            bool include = false;
            foreach (var item in Values)
            {
                if (isString)
                {
                    include = include || value.ToString().ToLower().Contains(item.ToLower().Trim());
                    continue;
                }
                
                if (isNumber)
                {
                    var op = ">";
                    if (item.Trim().StartsWith("<"))
                    {
                        op = "<";
                    }

                    var match = Regex.Match(item.Substring(1), @"(-?\d+\.?\d*)");
                    double dataToCompare = 0;
                    if (!string.IsNullOrEmpty(match.Value))
                    {
                        double.TryParse(match.Value, out dataToCompare);
                        string unit = item.Substring(match.Index + 1 + match.Length).ToLower().Trim();
                        if (unit == "kb" || unit == "k")
                        {
                            dataToCompare = dataToCompare * 1024;
                        }
                        else if (unit == "mb" || unit == "m")
                        {
                            dataToCompare = dataToCompare * 1024 * 1024;
                        }
                        else if (unit == "gb" || unit == "g")
                        {
                            dataToCompare = dataToCompare * 1024 * 1024 * 1024;
                        }
                    }

                    if (op == ">")
                    {
                        include = include || double.Parse(value.ToString()) > dataToCompare;
                    }
                    else if (op == "<")
                    {
                        include = include || double.Parse(value.ToString()) < dataToCompare;
                    }

                    continue;
                }

                if (isBoolean)
                {
                    bool valueToCompare = item.ToString().ToLower() == "true";
                    include = include || value.ToString().Equals(valueToCompare.ToString(), StringComparison.OrdinalIgnoreCase);
                    continue;
                }
            }

            return include;
        }

        private object GetFiledValue(APAsset asset)
        {
            var prefix = asset.GetType().Name.ToLower();
            var commonField = new string[] { "id", "name", "path", "filesize", "used", "hash" };
            if (commonField.Any(f => f.Equals(this.FieldKey, StringComparison.OrdinalIgnoreCase)))
            {
                prefix = "apasset";
            }

            var key = (prefix + "_" + this.FieldKey).ToLower();
            if (TableDefinitions.ActionsMap.ContainsKey(key))
            {
                return TableDefinitions.ActionsMap[key].GetRawData(asset);
            }

            return null;
        }

        public static List<SearchCondition> Parse(string expression)
        {
            if (string.IsNullOrEmpty(expression.Trim()))
            {
                return new List<SearchCondition>();
            }

            List<SearchCondition> list = new List<SearchCondition>();
            var pattern = @"(\w+?\W*?:)";
            
            if (Regex.IsMatch(expression, pattern))
            {
                var matches = Regex.Matches(expression, pattern);
                foreach (Match item in matches)
                {
                    var scondition = new SearchCondition();
                    scondition.FieldKey = expression.Substring(item.Index, item.Length - 1).Trim();
                    var nextMatch = item.NextMatch();
                    var valuePart = string.Empty;
                    if (nextMatch.Length == 0)
                    {
                        valuePart = expression.Substring(item.Index + item.Length, expression.Length - item.Index - item.Length);
                    }
                    else
                    {
                        valuePart = expression.Substring(item.Index + item.Length, nextMatch.Index - item.Index - item.Length);
                    }

                    valuePart = valuePart.Trim();
                    scondition.Values = valuePart.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    list.Add(scondition);
                }
                return list;
            }
            
            var condition = new SearchCondition();
            condition.FieldKey = "Name";
            condition.Values = expression.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            list.Add(condition);

            return list;
        }
    }
}