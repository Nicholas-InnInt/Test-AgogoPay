using Abp.Domain.Entities;
using Abp.Json;
using Castle.Components.DictionaryAdapter.Xml;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Neptune.NsPay.Merchants;
using Neptune.NsPay.PayGroupMents;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.WithdrawalDevices;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.DataEvent
{
    public class DataChangedEvent : IDataChangedEvent
    {
        private event EventHandler<MerchantPaymentEventArgs> onMerchantPaymentChanged;

        private event EventHandler<WithdrawalDeviceEventArgs> onWithdrawalDeviceChanged;
        private  ConcurrentDictionary<string,DateTime> partyDicts = new ConcurrentDictionary<string, DateTime>() ;

        public void SQLSugarChanged(string sqlQuery)
        {

        }

        public void MongoDbchange<T>(T newData, DataChangeType type)
        {
        }


        public void EFCoreChanged(DbContext tableName)
        {
            var affectedPaymentList = new MerchantPaymentEventArgs();
            var withdrawalDeviceList = new WithdrawalDeviceEventArgs();

            foreach (var entry in tableName.ChangeTracker.Entries())
            {
                if( entry.State == EntityState.Added || entry.State == EntityState.Deleted|| entry.State == EntityState.Modified)
                {
                    switch(entry.Entity.GetType())
                    {
                        case Type t when t == typeof(PayMent):

                            affectedPaymentList.Payment.Add(ConvertToObject<PayMent>(entry));
                            break;
                        case Type t when t == typeof(Merchant):
                            affectedPaymentList.Merchant.Add(ConvertToObject<Merchant>(entry));
                            break;
                        case Type t when t == typeof(PayGroupMent):
                            affectedPaymentList.PayGroupMent.Add(ConvertToObject<PayGroupMent>(entry));
                            break;
                        case Type t when t == typeof(WithdrawalDevice):
                            withdrawalDeviceList.WithdrawalDevice.Add(ConvertToObject<WithdrawalDevice>(entry));
                            break;
                    }
                }
            }


            if(affectedPaymentList.PayGroupMent.Count!=0 || affectedPaymentList.Payment.Count != 0|| affectedPaymentList.Merchant.Count != 0)
            {
                onMerchantPaymentChanged?.Invoke(this, affectedPaymentList);
            }


            if(withdrawalDeviceList.WithdrawalDevice.Count!=0)
            {
                onWithdrawalDeviceChanged?.Invoke(this, withdrawalDeviceList);
            }
        }

        private ChangedDataSet<T> ConvertToObject<T>(EntityEntry entityEntry) where T : new()
        {
            ChangedDataSet<T> returnValue = new ChangedDataSet<T>();

            try
            {
                if(entityEntry.State != EntityState.Added)
                {
                    T oldData = new T();
                    foreach (var property in entityEntry.OriginalValues.Properties)
                    {
                        var propertyName = property.Name;
                        var value = entityEntry.OriginalValues[propertyName];

                        // Use reflection to set the property value on the new entity
                        var propertyInfo = typeof(T).GetProperty(propertyName);
                        if (propertyInfo != null && propertyInfo.CanWrite)
                        {
                            propertyInfo.SetValue(oldData, value);
                        }
                    }
                    returnValue.OldData = oldData;
                }

                if(entityEntry.State != EntityState.Deleted)
                {
                    T newData = new T();
                    bool hadDeleted = false;

                    foreach (var property in entityEntry.CurrentValues.Properties)
                    {
                        var propertyName = property.Name;
                        var value = entityEntry.CurrentValues[propertyName];

                        // Use reflection to set the property value on the new entity
                        var propertyInfo = typeof(T).GetProperty(propertyName);
                        if (propertyInfo != null && propertyInfo.CanWrite)
                        {
                            propertyInfo.SetValue(newData, value);
                        }

                        if(propertyName == "IsDeleted" && value is bool booleanValue && booleanValue)
                        {
                            hadDeleted = true;
                        }
                    }


                    if(!hadDeleted) // Handle SoftDelete
                    {
                        returnValue.NewData = newData;
                    }
                }


            }
            catch(Exception ex)
            {

            }

            return returnValue;
        }


        public void AddMerchantPaymentChanged(EventHandler<MerchantPaymentEventArgs> handler, string uniqueID)
        {

            if(!string.IsNullOrEmpty(uniqueID)&& !partyDicts.TryGetValue(uniqueID , out DateTime insertDate))
            {
                partyDicts.TryAdd(uniqueID, DateTime.Now);
                onMerchantPaymentChanged += handler;
            }
            else if(string.IsNullOrEmpty(uniqueID))
            {
                onMerchantPaymentChanged += handler;
            }
        }

        public void AddWithdrawalDeviceChanged(EventHandler<WithdrawalDeviceEventArgs> handler, string uniqueID)
        {
            onWithdrawalDeviceChanged += handler;
        }
    }


    public enum DataChangeType
    {
        Add = 1,
        Update = 2,
        Delete = 3
    }

    public class MerchantPaymentEventArgs
    {
        public List<ChangedDataSet<PayMent>> Payment { get; set; }
        public List<ChangedDataSet<Merchant>> Merchant { get; set; }
        public List<ChangedDataSet<PayGroupMent>> PayGroupMent { get; set; }

        public MerchantPaymentEventArgs()
        {
            Payment = new List<ChangedDataSet<PayMent>>();
            Merchant = new List<ChangedDataSet<Merchant>>();
            PayGroupMent = new List<ChangedDataSet<PayGroupMent>>();
        }
    }

    public class WithdrawalDeviceEventArgs
    {
        public List<ChangedDataSet<WithdrawalDevice>> WithdrawalDevice { get; set; }
        public WithdrawalDeviceEventArgs()
        {
            WithdrawalDevice = new List<ChangedDataSet<WithdrawalDevice>>();
        }
    }
    public class ChangedDataSet<T>
    {
        public T OldData { get; set; }

        public T NewData { get; set; }
        public ChangedDataSet()
        {

        }
    }
    public class ChangedDataDetail<T>
    {
        public DataChangeType ChangeType { get; set; }

        public T NewData { get; set; }
        public ChangedDataDetail()
        {

        }
    }



    public interface IDataChangedEvent
    {
        void SQLSugarChanged(string sqlQuery);
        void EFCoreChanged(DbContext tableName);
        void MongoDbchange<T>(T newData, DataChangeType type);
        void AddMerchantPaymentChanged(EventHandler<MerchantPaymentEventArgs> handler , string uniqueID);
 		void AddWithdrawalDeviceChanged(EventHandler<WithdrawalDeviceEventArgs> handler, string uniqueID);
    }
}