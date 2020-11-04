using Plugins.CountlySDK.Models;
using Plugins.CountlySDK.Persistance.Dao;
using Plugins.CountlySDK.Persistance.Entities;

namespace Plugins.CountlySDK.Helpers
{
    public class EventNumberInSameSessionHelper
    {
        public const string NumberInSameSessionSegment = "numberInSameSession";
        
        private readonly EventNumberInSameSessionDao _dao;

        public EventNumberInSameSessionHelper(EventNumberInSameSessionDao dao)
        {
            _dao = dao;
        }
        
        public void IncreaseNumberInSameSession(CountlyEventModel @event)
        {
            var entity = IncrementEventNumberInSameSessionAndSaveOrUpdate(@event.Key);
            AddNumberInSameSessionToEvent(@event, entity.Number);
        }

        public void RemoveAllEvents()
        {
            _dao.RemoveAll();
        }

        private EventNumberInSameSessionEntity IncrementEventNumberInSameSessionAndSaveOrUpdate(string eventKey)
        {
            var entity = _dao.GetByEventName(eventKey);
            if (entity == null)
            {
                const int number = 1;
                entity = new  EventNumberInSameSessionEntity
                {
                    Id = _dao.GenerateNewId(),
                    EventKey = eventKey,
                    Number = number
                };
                _dao.Save(entity);
                return entity;
            }

            entity.Number++;
            _dao.Update(entity);

            return entity;
        }

        private void AddNumberInSameSessionToEvent(CountlyEventModel @event, int number)
        {
            if (@event.Segmentation == null)
            {
                @event.Segmentation = new SegmentModel();
            }
            @event.Segmentation.Add(NumberInSameSessionSegment, number);
        }
    }
}