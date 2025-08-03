using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Warehouse.Client.Services
{
    using Warehouse.Shared.DTO;
    using Warehouse.Shared.Filters;

    public readonly record struct ActionResult(ResultCode Result);

    public interface IApiAccess : IBalanceService, IClientService, IResourceService, IShipmentService, ISupplyService,
        IUnitService
    {
    }

    public class ApiAccess(IHttpClientFactory httpClientFactory) : IApiAccess
    {
        public const string ApiClientName = "API";

        private HttpClient Client => httpClientFactory.CreateClient(ApiClientName);

        #region Low‑level helpers

        /// <summary>
        /// Safely reads JSON from an <see cref="HttpContent"/>. Returns <c>default</c> if the payload is empty or cannot be deserialized.
        /// </summary>
        private static async Task<TRes?> TryReadJsonAsync<TRes>(HttpContent content)
        {
            if (content == null)
            {
                return default;
            }

            try
            {
                return await content.ReadFromJsonAsync<TRes>();
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// Maps an <see cref="HttpResponseMessage"/> to a <see cref="ResultCode"/> and deserialized body.
        /// </summary>
        private static async Task<(ResultCode, TRes?)> MapResponseAsync<TRes>(HttpResponseMessage response)
        {
            var data = await TryReadJsonAsync<TRes>(response.Content);

            if (response.IsSuccessStatusCode)
            {
                return (ResultCode.Ok, data);
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return (ResultCode.Unauthorized, data);
            }
            else
            {
                return (ResultCode.Error, data);
            }
        }

        /// <summary>
        /// Формирует строку запроса, преобразуя публичные свойства фильтра в параметры query-string.
        /// </summary>
        /// <typeparam name="T">Тип объекта фильтра.</typeparam>
        /// <param name="baseUri">Базовый URI, к которому будут добавлены параметры запроса.</param>
        /// <param name="filter">Объект фильтра, свойства которого будут сериализованы в параметры запроса.</param>
        /// <returns>
        /// URI с добавленными параметрами запроса для всех непустых свойств фильтра.
        /// Для скалярных типов (<see cref="string"/>, <see cref="bool"/>, <see cref="DateTime"/>) добавляются одиночные параметры,
        /// для коллекций (<see cref="IEnumerable{Guid}"/>, <see cref="IEnumerable{string}"/>) — повторяющиеся ключи.
        /// </returns>
        /// <remarks>
        /// Поддерживаемые типы свойств:
        /// <list type="bullet">
        ///   <item><description><see cref="string"/> — непустая строка добавляется как один параметр.</description></item>
        ///   <item><description><see cref="bool"/> — добавляется как <see langword="true"/> или <see langword="false"/>.</description></item>
        ///   <item><description><see cref="DateTime"/> — добавляется в формате ISO 8601 (ToString(\"o\")).</description></item>
        ///   <item><description><see cref="IEnumerable{Guid}"/> и <see cref="IEnumerable{string}"/> — каждая запись коллекции добавляется отдельным параметром.</description></item>
        /// </list>
        /// </remarks>
        private static string BuildQuery<T>(string baseUri, T filter)
        {
            var simple = new Dictionary<string, string?>();
            var multi = new Dictionary<string, StringValues>();

            foreach (var prop in typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var val = prop.GetValue(filter);

                if (val == null)
                {
                    continue;
                }

                switch (val)
                {
                    case string s:
                        if (!string.IsNullOrEmpty(s))
                        {
                            simple[prop.Name] = s;
                        }
                        break;

                    case bool b:
                        simple[prop.Name] = b.ToString();
                        break;

                    case DateTime dt:
                        simple[prop.Name] = dt.ToString("o");
                        break;

                    case IEnumerable<Guid> guids:
                        var guArr = guids
                            .Where(g => g != Guid.Empty)
                            .Select(g => g.ToString())
                            .ToArray();
                        if (guArr.Length > 0)
                        {
                            multi[prop.Name] = new StringValues(guArr);
                        }
                        break;

                    case IEnumerable<string> strs:
                        var stArr = strs
                            .Where(x => !string.IsNullOrEmpty(x))
                            .ToArray();
                        if (stArr.Length > 0)
                        {
                            multi[prop.Name] = new StringValues(stArr);
                        }
                        break;
                }
            }

            var uri = QueryHelpers.AddQueryString(baseUri, simple);
            return QueryHelpers.AddQueryString(uri, multi);
        }

        #endregion

        #region Public API wrappers

        private async Task<ResultCode> DeleteAsync(string uri)
        {
            var response = await Client.DeleteAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                return ResultCode.Ok;
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return ResultCode.Unauthorized;
            }
            else
            {
                return ResultCode.Error;
            }
        }

        private async Task<(ResultCode, TRes?)> GetAsync<TRes>(string uri)
        {
            var response = await Client.GetAsync(uri);

            return await MapResponseAsync<TRes>(response);
        }

        private async Task<ResultCode> PostAsync<TBody>(string uri, TBody body)
        {
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(uri, content);

            if (response.IsSuccessStatusCode)
            {
                return ResultCode.Ok;
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return ResultCode.Unauthorized;
            }
            else
            {
                return ResultCode.Error;
            }
        }

        private async Task<(ResultCode, TRes?)> PostAsync<TBody, TRes>(string uri, TBody body)
        {
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(uri, content);

            return await MapResponseAsync<TRes>(response);
        }

        private async Task<ResultCode> PutAsync<TBody>(string uri, TBody body)
        {
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await Client.PutAsync(uri, content);

            if (response.IsSuccessStatusCode)
            {
                return ResultCode.Ok;
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return ResultCode.Unauthorized;
            }
            else
            {
                return ResultCode.Error;
            }
        }

        private async Task<(ResultCode, TRes?)> PutAsync<TBody, TRes>(string uri, TBody body)
        {
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await Client.PutAsync(uri, content);

            return await MapResponseAsync<TRes>(response);
        }

        #endregion

        public async Task<GetBalancesResult> GetAllBalances(BalanceFilter filter)
        {
            var uri = BuildQuery("balance", filter);
            var (code, list) = await GetAsync<IEnumerable<BalanceDTO>>(uri);
            return new GetBalancesResult(code, list);
        }

        public async Task<GetClientsResult> GetClients(bool includeArchived = false)
        {
            var uri = $"client?includeArchived={includeArchived}";
            var (code, list) = await GetAsync<IEnumerable<ClientDTO>>(uri);
            return new GetClientsResult(code, list);
        }

        public async Task<GetClientResult> GetClient(Guid id)
        {
            var (code, dto) = await GetAsync<ClientDTO>($"client/{id}");
            return new GetClientResult(code, dto);
        }

        public async Task<CreateClientResult> CreateClient(ClientDTO dto)
        {
            var (code, client) = await PostAsync<ClientDTO, ClientDTO>("client", dto);
            return new CreateClientResult(code, client);
        }

        public async Task<ActionResult> UpdateClient(ClientDTO dto)
        {
            var code = await PutAsync($"client/{dto.Id}", dto);
            return new ActionResult(code);
        }

        public async Task<ActionResult> DeleteClient(Guid id)
        {
            var code = await DeleteAsync($"client/{id}");
            return new ActionResult(code);
        }

        public async Task<GetResourcesResult> GetAllResources(bool includeArchived = false)
        {
            var uri = $"resource?includeArchived={includeArchived}";
            var (code, list) = await GetAsync<IEnumerable<ResourceDTO>>(uri);
            return new GetResourcesResult(code, list);
        }

        public async Task<GetResourceResult> GetResource(Guid id)
        {
            var (code, dto) = await GetAsync<ResourceDTO>($"resource/{id}");
            return new GetResourceResult(code, dto);
        }

        public async Task<CreateResourceResult> Create(ResourceDTO dto)
        {
            var (code, resource) = await PostAsync<ResourceDTO, ResourceDTO>("resource", dto);
            return new CreateResourceResult(code, resource);
        }

        public async Task<ActionResult> Update(ResourceDTO dto)
        {
            var code = await PutAsync($"resource/{dto.Id}", dto);
            return new ActionResult(code);
        }

        public async Task<ActionResult> Archive(Guid id)
        {
            var code = await DeleteAsync($"resource/{id}");
            return new ActionResult(code);
        }

        public async Task<GetShipmentsResult> GetAllShipments(DocumentFilter filter)
        {
            var uri = BuildQuery("shipment", filter);
            var (code, list) = await GetAsync<IEnumerable<ShipmentDocumentDTO>>(uri);
            return new GetShipmentsResult(code, list);
        }

        public async Task<GetShipmentResult> GetShipment(Guid id)
        {
            var (code, doc) = await GetAsync<ShipmentDocumentDTO>($"shipment/{id}");
            return new GetShipmentResult(code, doc);
        }

        public async Task<CreateShipmentResult> Create(ShipmentDocumentDTO dto)
        {
            var (code, shipment) = await PostAsync<ShipmentDocumentDTO, ShipmentDocumentDTO>("shipment", dto);
            return new CreateShipmentResult(code, shipment);
        }

        public async Task<ActionResult> Update(ShipmentDocumentDTO dto)
        {
            var code = await PutAsync($"shipment/{dto.Id}", dto);
            return new ActionResult(code);
        }

        public async Task<ActionResult> Delete(Guid id)
        {
            var code = await DeleteAsync($"shipment/{id}");
            return new ActionResult(code);
        }

        public async Task<ActionResult> Sign(Guid id)
        {
            var code = await PostAsync($"shipment/{id}/sign", string.Empty);
            return new ActionResult(code);
        }

        public async Task<ActionResult> Revoke(Guid id)
        {
            var code = await PostAsync($"shipment/{id}/revoke", string.Empty);
            return new ActionResult(code);
        }

        public async Task<GetSuppliesResult> GetAllSupplies(DocumentFilter filter)
        {
            var uri = BuildQuery("supply", filter);
            var (code, list) = await GetAsync<IEnumerable<SupplyDocumentDTO>>(uri);
            return new GetSuppliesResult(code, list);
        }

        public async Task<GetSupplyResult> GetSupply(Guid id)
        {
            var (code, supply) = await GetAsync<SupplyDocumentDTO>($"supply/{id}");
            return new GetSupplyResult(code, supply);
        }

        public async Task<CreateSupplyResult> Create(SupplyDocumentDTO dto)
        {
            var (code, supply) = await PostAsync<SupplyDocumentDTO, SupplyDocumentDTO>("supply", dto);
            return new CreateSupplyResult(code, supply);
        }

        public async Task<ActionResult> Update(SupplyDocumentDTO dto)
        {
            var code = await PutAsync($"supply/{dto.Id}", dto);
            return new ActionResult(code);
        }

        public async Task<ActionResult> DeleteSupply(Guid id)
        {
            var code = await DeleteAsync($"supply/{id}");
            return new ActionResult(code);
        }

        public async Task<GetUnitsResult> GetAllUnits(bool includeArchived)
        {
            var uri = $"unit?includeArchived={includeArchived}";
            var (code, list) = await GetAsync<IEnumerable<UnitDTO>>(uri);
            return new GetUnitsResult(code, list);
        }

        public async Task<GetUnitResult> GetUnit(Guid id)
        {
            var (code, unit) = await GetAsync<UnitDTO>($"unit/{id}");
            return new GetUnitResult(code, unit);
        }

        public async Task<CreateUnitResult> Create(UnitDTO dto)
        {
            var (code, unit) = await PostAsync<UnitDTO, UnitDTO>("unit", dto);
            return new CreateUnitResult(code, unit);
        }

        public async Task<ActionResult> Update(UnitDTO dto)
        {
            var code = await PutAsync($"unit/{dto.Id}", dto);
            return new ActionResult(code);
        }

        public async Task<ActionResult> ArchiveUnit(Guid id)
        {
            var code = await DeleteAsync($"unit/{id}");
            return new ActionResult(code);
        }
    }
}
