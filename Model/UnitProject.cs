using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Transform
{
    public class UnitProject
    {
        public int UnitInt { get; set; }
        public string UnitName { get; set; }
        public UnitProject(int unitInt, string unitName)
        {
            UnitInt = unitInt;
            UnitName = unitName;
        }
        public double Convert(double a)
        {
            if (UnitName.Equals("cm"))
            {
                a = UnitUtils.Convert(a, UnitTypeId.Feet, UnitTypeId.Centimeters);
            }
            if (UnitName.Equals("dm"))
            {
                a = UnitUtils.Convert(a, UnitTypeId.Feet, UnitTypeId.Decimeters);
            }
            if (UnitName.Equals("ft"))
            {
                a = UnitUtils.Convert(a, UnitTypeId.Feet, UnitTypeId.Feet);
            }
            if (UnitName.Equals("in"))
            {
                a = UnitUtils.Convert(a, UnitTypeId.Feet, UnitTypeId.Inches);
            }
            if (UnitName.Equals("m"))
            {
                a = UnitUtils.Convert(a, UnitTypeId.Feet, UnitTypeId.Meters);
            }
            if (UnitName.Equals("mm"))
            {
                a = UnitUtils.Convert(a, UnitTypeId.Feet, UnitTypeId.Millimeters);
            }
            if (UnitName.Equals("inUS"))
            {
                a = UnitUtils.Convert(a, UnitTypeId.Feet, UnitTypeId.Inches);
            }
            if (UnitName.Equals("ft-in"))
            {
                a = UnitUtils.Convert(a, UnitTypeId.Feet, UnitTypeId.FeetFractionalInches);
            }
            if (UnitName.Equals("inch"))
            {
                a = UnitUtils.Convert(a, UnitTypeId.Feet, UnitTypeId.FractionalInches);
            }
            if (UnitName.Equals("m"))
            {
                a = UnitUtils.Convert(a, UnitTypeId.Feet, UnitTypeId.Meters);
            }
            return a;


        }
        public static UnitProject GetUnitProject(Document document)
        {
            UnitProject a = new UnitProject(1, "ft");

            ForgeTypeId forgeTypeId = document.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId();
            if (forgeTypeId == UnitTypeId.Centimeters)
            {
                a.UnitInt = 1; a.UnitName = "cm";
            }
            if (forgeTypeId == UnitTypeId.Decimeters)
            {
                a.UnitInt = 2; a.UnitName = "dm";
            }
            if (forgeTypeId == UnitTypeId.Feet)
            {
                a.UnitInt = 3; a.UnitName = "ft";
            }
            if (forgeTypeId == UnitTypeId.Inches)
            {
                a.UnitInt = 4; a.UnitName = "in";
            }
            if (forgeTypeId == UnitTypeId.Meters)
            {
                a.UnitInt = 5; a.UnitName = "m";
            }
            if (forgeTypeId == UnitTypeId.Millimeters)
            {
                a.UnitInt = 6; a.UnitName = "mm";
            }
            if (forgeTypeId == UnitTypeId.Inches)
            {
                a.UnitInt = 7; a.UnitName = "inUS";
            }
            if (forgeTypeId == UnitTypeId.FeetFractionalInches)
            {
                a.UnitInt = 8; a.UnitName = "ft-in";
            }
            if (forgeTypeId == UnitTypeId.FractionalInches)
            {
                a.UnitInt = 9; a.UnitName = "inch";
            }
            if (forgeTypeId == UnitTypeId.MetersCentimeters)
            {
                a.UnitInt = 10; a.UnitName = "m";
            }
            return a;
        }
    }
}
