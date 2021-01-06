using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDAApi.Utility
{
    public class SettingValidationStartupFilter: IStartupFilter
    {
        readonly IEnumerable<IValidatable> _validatableObjects;
        public SettingValidationStartupFilter(IEnumerable<IValidatable> validatableObjects)
        {
            _validatableObjects = validatableObjects;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            foreach (var validatableObject in _validatableObjects)
            {
                validatableObject.Validate();
            }
            return next;
        }

       
    }
}
