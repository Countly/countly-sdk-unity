namespace Plugins.CountlySDK.Persistance.Entities
{
    public class RequestEntity : IEntity
    {
        public long Id;
        public string Json;
        
        public long GetId()
        {
            return Id;
        }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Json)}: {Json}";
        }

        private bool Equals(RequestEntity other)
        {
            return Id == other.Id && string.Equals(Json, other.Json);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((RequestEntity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Id.GetHashCode() * 397) ^ (Json != null ? Json.GetHashCode() : 0);
            }
        }
    }
}