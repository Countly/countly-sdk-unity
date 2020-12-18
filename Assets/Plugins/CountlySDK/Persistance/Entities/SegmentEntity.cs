namespace Plugins.CountlySDK.Persistance.Entities
{
    public class SegmentEntity : IEntity
    {
        public long Id;
        public string Json;
        public long EventId;

        public long GetId()
        {
            return Id;
        }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Json)}: {Json}, {nameof(EventId)}: {EventId}";
        }

        private bool Equals(SegmentEntity other)
        {
            return Id == other.Id && string.Equals(Json, other.Json) && EventId == other.EventId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return Equals((SegmentEntity)obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                int hashCode = Id.GetHashCode();
                hashCode = (hashCode * 397) ^ (Json != null ? Json.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ EventId.GetHashCode();
                return hashCode;
            }
        }
    }
}