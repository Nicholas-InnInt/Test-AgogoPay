﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.DynamicEntityProperties;
using Abp.UI.Inputs;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.DynamicEntityProperties.Dto;

namespace Neptune.NsPay.DynamicEntityProperties
{
    [AbpAuthorize(AppPermissions.Pages_Administration_DynamicProperties)]
    public class DynamicPropertyAppService : NsPayAppServiceBase, IDynamicPropertyAppService
    {
        private readonly IDynamicPropertyManager _dynamicPropertyManager;
        private readonly IDynamicPropertyStore _dynamicPropertyStore;
        private readonly IDynamicEntityPropertyDefinitionManager _dynamicEntityPropertyDefinitionManager;

        public DynamicPropertyAppService(
            IDynamicPropertyManager dynamicPropertyManager,
            IDynamicPropertyStore dynamicPropertyStore,
            IDynamicEntityPropertyDefinitionManager dynamicEntityPropertyDefinitionManager)
        {
            _dynamicPropertyManager = dynamicPropertyManager;
            _dynamicPropertyStore = dynamicPropertyStore;
            _dynamicEntityPropertyDefinitionManager = dynamicEntityPropertyDefinitionManager;
        }

        public async Task<DynamicPropertyDto> Get(int id)
        {
            var entity = await _dynamicPropertyManager.GetAsync(id);
            return ObjectMapper.Map<DynamicPropertyDto>(entity);
        }

        public async Task<ListResultDto<DynamicPropertyDto>> GetAll()
        {
            var entities = await _dynamicPropertyStore.GetAllAsync();

            return new ListResultDto<DynamicPropertyDto>(
                ObjectMapper.Map<List<DynamicPropertyDto>>(entities)
            );
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_DynamicPropertyValue_Create)]
        public async Task Add(DynamicPropertyDto dto)
        {
            dto.TenantId = AbpSession.TenantId;
            await _dynamicPropertyManager.AddAsync(ObjectMapper.Map<DynamicProperty>(dto));
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_DynamicPropertyValue_Edit)]
        public async Task Update(DynamicPropertyDto dto)
        {
            dto.TenantId = AbpSession.TenantId;
            await _dynamicPropertyManager.UpdateAsync(ObjectMapper.Map<DynamicProperty>(dto));
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_DynamicPropertyValue_Delete)]
        public async Task Delete(int id)
        {
            await _dynamicPropertyManager.DeleteAsync(id);
        }

        public IInputType FindAllowedInputType(string name)
        {
            return _dynamicEntityPropertyDefinitionManager.GetOrNullAllowedInputType(name);
        }
    }
}
