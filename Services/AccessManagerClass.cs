using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WildGrassPOSLibrary.Models;

namespace WildGrass_Desktop_f8.Services
{
    internal class AccessManagerClass
    {
        public bool checkValidity(string uid, BusinessClass business, int userType)
        {
            bool success = false;

            if (!business.businessShellYes)
            {
                //set dates
                DateTime dt = DateTime.Now;
                int month = dt.Month;
                month--;
                int year = dt.Year;
                int daySele = dt.Day;


                int yearv = Convert.ToInt32(business.validUntilYear);
                int monthv = Convert.ToInt32(business.validUntilMonth);
                int dayv = Convert.ToInt32(business.validUntilDay);
                bool validity = false;

                if (yearv > year)
                {
                    validity = true;
                }
                else if (yearv == year)
                {
                    if (monthv > month)
                    {
                        validity = true;
                    }
                    else if (monthv == month)
                    {
                        if (dayv >= daySele)
                        {
                            validity = true;
                        }
                    }
                }

                if (business.package == 0)
                {
                    if (business.accessLevel == "No Package Accessed")
                    {
                    }
                    else if (!validity)
                    {
                    }
                    else if (business.accessLevel == "Compact")
                    {
                    }
                    else
                    {
                        success = true;
                    }
                }
                else
                {
                    success = true;
                }
            }
            else
            {
                success = true;
            }

            return success;
        }
    }
}
