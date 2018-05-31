/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// This is a regression algorithm to test the price and split factors in coarse fundamental data
    /// </summary>
    /// <meta name="tag" content="using data" />
    /// <meta name="tag" content="universes" />
    /// <meta name="tag" content="coarse universes" />
    /// <meta name="tag" content="regression test" />
    public class CoarseUniverseSplitRegressionAlgorithm : QCAlgorithm
    {
        private static readonly Symbol Apple = QuantConnect.Symbol.Create("AAPL", SecurityType.Equity, Market.USA);

        private readonly Dictionary<DateTime, PriceData> _expectedData = new Dictionary<DateTime, PriceData>
        {
            { new DateTime(2014, 6, 6), new PriceData(647.35m, 0.9304792m, 0.142857m, 86.049301110612840m) },
            { new DateTime(2014, 6, 7), new PriceData(645.57m, 0.9304792m, 0.142857m, 85.812693779220408m) },
            { new DateTime(2014, 6, 10), new PriceData(93.7m, 0.9304792m, 1m, 87.18590104m) },
            { new DateTime(2014, 6, 11), new PriceData(94.25m, 0.9304792m, 1m, 87.697664600m) }
        };

        private readonly Dictionary<DateTime, PriceData> _actualData = new Dictionary<DateTime, PriceData>();

        /// <summary>
        /// Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.
        /// </summary>
        public override void Initialize()
        {
            UniverseSettings.Resolution = Resolution.Daily;

            SetStartDate(2014, 06, 5);
            SetEndDate(2014, 06, 11);
            SetCash(50000);

            AddUniverse(coarse =>
            {
                var cf = coarse.FirstOrDefault(x => x.Symbol == Apple);
                if (cf != null)
                {
                    _actualData.Add(cf.EndTime, new PriceData(cf.Price, cf.PriceFactor, cf.SplitFactor, cf.AdjustedPrice));

                    Log($"Symbol:{cf.Symbol.Value} EndTime:{cf.EndTime} RawPrice:{cf.Price} PriceFactor:{cf.PriceFactor} SplitFactor:{cf.SplitFactor} AdjustedPrice:{cf.AdjustedPrice}");
                }

                return new List<Symbol> { Apple };
            });
        }

        /// <summary>
        /// End of algorithm run event handler. This method is called at the end of a backtest or live trading operation.
        /// </summary>
        public override void OnEndOfAlgorithm()
        {
            foreach (var kvp in _actualData)
            {
                var time = kvp.Key;
                var data = kvp.Value;

                if (!data.AreEqual(_expectedData[time]))
                {
                    throw new Exception($"Data mismatch for date: {time}");
                }
            }
        }

        private class PriceData
        {
            private decimal RawPrice { get; }
            private decimal PriceFactor { get; }
            private decimal SplitFactor { get; }
            private decimal AdjustedPrice { get; }

            public PriceData(decimal rawPrice, decimal priceFactor, decimal splitFactor, decimal adjustedPrice)
            {
                RawPrice = rawPrice;
                PriceFactor = priceFactor;
                SplitFactor = splitFactor;
                AdjustedPrice = adjustedPrice;
            }

            public bool AreEqual(PriceData data)
            {
                return
                    data.RawPrice == RawPrice &&
                    data.PriceFactor == PriceFactor &&
                    data.SplitFactor == SplitFactor &&
                    data.AdjustedPrice == AdjustedPrice;
            }
        }
    }
}