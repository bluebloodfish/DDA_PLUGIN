using DDAApi.DataAccess;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDAApi.Utility
{
    public class DDAApiSetting: IValidatable
    {
        

        public static IHospMenuManager _hospMenuManager;
        public static ILogger<DDAApiSetting> _logger;
        //public static ISqlHttpCallback _sqlHttpCallback;

        public string T_AppId { get; set; }

        public string T_SecretKey { get; set; }

        [Url]
        public string T_CallBackBaseUrl { get; set; }

        [Required]
        public string AppId { get; set; }

        [Required]
        public string SecretKey { get; set; }

        //[Required]
        [StringLength(4, MinimumLength =0, ErrorMessage = "DefaultItemCode's max length is 4 characters")]
        public string DefaultItemCode { get; set; }

        //[Required]
        [StringLength(4, MinimumLength = 0, ErrorMessage = "DefaultInstructionCode's max length is 4 characters")]
        public string DefaultInstructionCode { get; set; }


        [StringLength(4, MinimumLength = 0, ErrorMessage = "DeliveryItemCode's max length is 4 characters")]
        public string DeliveryItemCode { get; set; }

        [Required]
        [StringLength(4, MinimumLength = 1, ErrorMessage = "SurchargeItemCode's max length is 4 characters")]
        public string SurchargeItemCode { get; set; }
        //[Required]
        [Range(1, 2)]
        [DefaultValue(1)]
        public int PrintDescNumberForSimpleOrder { get; set; }


        [Required]
        public string PrinterServer { get; set; }
        [DefaultValue(60606)]
        public int PrinterServerPort { get; set; }

        //[DefaultValue(0)]
        //public int AutoPrintBillForUnpaidOrder { get; set; }

        [DefaultValue(0)]
        public int AutoPrintBill { get; set; }

        [Range(0, 1)]
        [DefaultValue(1)]
        public int Print_OnlineOrderNo_inNotes { get; set; }

        [Range(0, 1)]
        [DefaultValue(1)]
        public int Print_OnlineOrderPayStatus_inNotes { get; set; }


        [Required]
        [DefaultValue(0)]
        public int EnableCancelOrderFunction { get; set; }

        [Required]
        [Url]
        public string SQLCallbackBaseUrl { get; set; }

        //1 = auto merge order, 0 = auto reject order, 2 = remind and buffer order for manually confirm 
        public int OrderForOccupiedTable { get; set; }

        //[Required]
        //[DefaultValue(2020)]
        //[Range(minimum:1990, maximum: 2999, ErrorMessage = "StartYear must be between 1990 and 2999.")]
        //public int OnlineOrderStartYear { get; set; }

        public void Validate()
        {

            Validator.ValidateObject(this, new ValidationContext(this), validateAllProperties: true);

            if (!string.IsNullOrEmpty(SurchargeItemCode) && !_hospMenuManager.GetMenuItems().Any(x => x.ItemCode.ToLower() == SurchargeItemCode.ToLower()))
            {
                throw new Exception($"Property '{nameof(SurchargeItemCode)}' does not exist in database!!");
            }

            if (!string.IsNullOrEmpty(DefaultItemCode) && !_hospMenuManager.GetMenuItems().Any(x => x.ItemCode.ToLower() == DefaultItemCode.ToLower()))
            {
                throw new Exception($"Property '{nameof(DefaultItemCode)}' does not exist in database!!");
            }

            if (!string.IsNullOrEmpty(DefaultInstructionCode) && !_hospMenuManager.GetMenuItems().Any(x => x.ItemCode.ToLower() == DefaultInstructionCode.ToLower()))
            {
                throw new Exception($"Property '{nameof(DefaultInstructionCode)}' does not exist in database!!");
            }

            if (!(!string.IsNullOrEmpty(T_AppId) && !string.IsNullOrEmpty(T_SecretKey) && !string.IsNullOrEmpty(T_CallBackBaseUrl)))
            {
                if (string.IsNullOrEmpty(T_AppId))
                {
                    throw new Exception($"T_AppId is required, cannot be empty.");
                }

                if (string.IsNullOrEmpty(T_SecretKey))
                {
                    throw new Exception($"T_SecretKey is required, cannot be empty.");
                }

                if (string.IsNullOrEmpty(T_CallBackBaseUrl))
                {
                    throw new Exception($"CallBackUrl is required, cannot be empty.");
                }

            }
            
        }
    }
}
