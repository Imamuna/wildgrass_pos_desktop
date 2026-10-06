using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WildGrass_Desktop;
using WildGrass_Desktop_f8.Functions;
using WildGrassPOSLibrary.Models;
using WildGrass_Desktop_f8.Services;

namespace WildGrass_Desktop
{
    class ProcessTransactionClass
    {
        public SummationClass SetSummations(Dictionary<string, OnGoingClass> onGoingArray, double discount, double addCost)
        {
            //currency
            WildGrassPOSLibrary.Services.PrevalentClass prevelantClass = new();
            SettingsClass settingsClass = prevelantClass.RetrieveSettings();
            double vatRate = 16;

            if (settingsClass != null) if (settingsClass.bookKeepingSettings != null)
                {
                    vatRate = settingsClass.bookKeepingSettings.vat;
                }

            SummationClass summation = new SummationClass();

            if (onGoingArray != null && onGoingArray.Count > 0)
            {
                foreach (var item in onGoingArray)
                {
                    double count = Convert.ToDouble(item.Value.quantity);
                    double price = Convert.ToDouble(item.Value.price);

                    double subtotal = count * price;

                    //discount
                    switch (item.Value.discountYes)
                    {
                        case 0://no entry
                            break;
                        case 1://percentage
                            double value1 = item.Value.discountAmount * subtotal / 100;
                            summation.IndDiscount += value1;
                            break;
                        case 2://absolute
                            summation.IndDiscount += item.Value.discountAmount;
                            break;
                    }

                    //addCost
                    switch (item.Value.addCostYes)
                    {
                        case 0://no entry
                            break;
                        case 1://percentage
                            double value2 = item.Value.addCostAmount * subtotal / 100;
                            summation.IndAddCost += value2;
                            break;
                        case 2://absolute
                            summation.IndAddCost += item.Value.addCostAmount;
                            break;
                    }

                    //tax
                    int vatType = item.Value.vatType;
                    switch (vatType)
                    {
                        case 1://included vat
                            summation.Incl += (subtotal / (100 + vatRate))* vatRate;
                            break;
                        case 2://excluded vat
                            summation.Excl += subtotal * vatRate / 100;
                            break;
                    }

                    summation.Cart += subtotal;
                }
                summation.globalAddCost = addCost;
                summation.globalDiscount = discount;
                summation.Discount = summation.globalDiscount + summation.IndDiscount;
                summation.AddCost = summation.globalAddCost + summation.IndAddCost;
                summation.Vat = summation.Incl + summation.Excl;
                summation.Total = summation.Cart + summation.Excl + summation.AddCost - summation.Discount;
            }

            return summation;
        }
        public SummationClass SetSummations(Dictionary<string, ProductEntrySDClass> onGoingArray, double discount, double addCost)
        {
            //currency
            WildGrassPOSLibrary.Services.PrevalentClass prevelantClass = new();
            SettingsClass settingsClass = prevelantClass.RetrieveSettings();
            double vatRate = 16;

            if (settingsClass != null) if (settingsClass.bookKeepingSettings != null)
                {
                    vatRate = settingsClass.bookKeepingSettings.vat;
                }

            SummationClass summation = new SummationClass();

            if (onGoingArray != null && onGoingArray.Count > 0)
            {
                foreach (var item in onGoingArray)
                {
                    double count = Convert.ToDouble(item.Value.quantity);
                    double price = Convert.ToDouble(item.Value.price);

                    double subtotal = count * price;

                    //discount
                    switch (item.Value.discountYes)
                    {
                        case 0://no entry
                            break;
                        case 1://percentage
                            double value1 = Convert.ToDouble(item.Value.discountAmount) * subtotal / 100;
                            summation.IndDiscount += value1;
                            break;
                        case 2://absolute
                            summation.IndDiscount += item.Value.discountAmount;
                            break;
                    }

                    //addCost
                    switch (item.Value.addCostYes)
                    {
                        case 0://no entry
                            break;
                        case 1://percentage
                            double value2 = Convert.ToDouble(item.Value.addCostAmount) * subtotal / 100;
                            summation.IndAddCost += value2;
                            break;
                        case 2://absolute
                            summation.IndAddCost += item.Value.addCostAmount;
                            break;
                    }

                    //tax
                    int vatType = item.Value.vatType;
                    switch (vatType)
                    {
                        case 1://included vat
                            summation.Incl += (subtotal / (100 + vatRate))* vatRate;
                            break;
                        case 2://excluded vat
                            summation.Excl += subtotal * vatRate / 100;
                            break;
                    }

                    summation.Cart += subtotal;
                }
                summation.globalAddCost = addCost;
                summation.globalDiscount = discount;
                summation.Discount = summation.globalDiscount + summation.IndDiscount;
                summation.AddCost = summation.globalAddCost + summation.IndAddCost;
                summation.Vat = summation.Incl + summation.Excl;
                summation.Total = summation.Cart + summation.Excl + summation.AddCost - summation.Discount;
            }

            return summation;
        }
    }
}
