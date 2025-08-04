namespace Warehouse.Server
{
    public class SlugifyParameterTransformer : IOutboundParameterTransformer
    {
        public string? TransformOutbound(object? value)
        {
            if (value == null)
            {
                return null;
            }

            // Преобразует в нижний регистр
            return value.ToString()?.ToLowerInvariant();
        }
    }
}
