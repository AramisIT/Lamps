using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WMS_client.db;
using WMS_client.Enums;
using WMS_client.Models;

namespace WMS_client
    {
    static class AccessoryHelper
        {
        public static string GetModelDescription(this IAccessory accessory)
            {
            return Configuration.Current.Repository.GetModel(accessory.Model).Description;
            }

        public static string GetPartyDescription(this IAccessory accessory)
            {
            return Configuration.Current.Repository.GetParty(accessory.Party).Description;
            }

        public static string GetPartyContractor(this IAccessory accessory)
            {
            return Configuration.Current.Repository.GetParty(accessory.Party).ContractorDescription;
            }

        public static string GetPartyDate(this IAccessory accessory)
            {
            return Configuration.Current.Repository.GetParty(accessory.Party).Date.ToString("dd.MM.yyyy");
            }

        public static string GetWarrantyExpiryDate(this IAccessory accessory)
            {
            return accessory.WarrantyExpiryDate.ToString("dd.MM.yyyy");
            }

        public static string GetStatusDescription(this IAccessory accessory)
            {
            return getEnumDescription<TypesOfLampsStatus>(accessory.Status);
            }

        public static bool HasNullWarrantyExpiryDate(this IAccessory accessory)
            {
            return accessory.WarrantyExpiryDate.Equals(DateTime.MinValue);
            }

        private static string getEnumDescription<T>(byte enumValue)
            {
            var enumDescriptions = EnumWorker.GetList(typeof(T));

            string description;
            if (enumDescriptions.TryGetValue(enumValue, out description))
                {
                return description;
                }

            return string.Empty;
            }

        public static string GetWarrantyType(this IAccessory accessory)
            {
            TypesOfLampsWarrantly warrantyType;

            if (accessory.HasNullWarrantyExpiryDate())
                {
                warrantyType = TypesOfLampsWarrantly.Without;
                }
            else
                {
                var fixableAccessory = accessory as IFixableAccessory;

                if (fixableAccessory == null || !fixableAccessory.RepairWarranty)
                    {
                    warrantyType = TypesOfLampsWarrantly.Factory;
                    }
                else
                    {
                    warrantyType = TypesOfLampsWarrantly.Repair;
                    }
                }

            return getEnumDescription<TypesOfLampsWarrantly>((byte)warrantyType);
            }

        public static TypeOfAccessories GetAccessoryType(this IAccessory accessory)
            {
            if (accessory is Case)
                {
                return TypeOfAccessories.Case;
                }
            else if (accessory is Unit)
                {
                return TypeOfAccessories.ElectronicUnit;
                }
            else
                {
                return TypeOfAccessories.Lamp;
                }
            }

        internal static IAccessory CreateNewAccessory(int barcodeInt, TypeOfAccessories requaredAccessoryType)
            {
            switch (requaredAccessoryType)
                {
                case TypeOfAccessories.Case:
                    return new Case() { Id = barcodeInt };

                case TypeOfAccessories.ElectronicUnit:
                    return new Unit() { Barcode = barcodeInt };

                case TypeOfAccessories.Lamp:
                    return new Lamp() { Barcode = barcodeInt };

                default:
                    return null;
                }
            }

        internal static bool HasBarcode(this IAccessory accessory, string barcode)
            {
            if (accessory == null)
                {
                return false;
                }

            int barcodeInt = barcode.GetIntegerBarcode();
            IBarcodeAccessory iBarcodeAccessory = accessory as IBarcodeAccessory;

            if (iBarcodeAccessory == null)
                {
                return accessory.Id == barcodeInt;
                }
            else
                {
                return iBarcodeAccessory.Barcode == barcodeInt;
                }
            }
        }
    }
