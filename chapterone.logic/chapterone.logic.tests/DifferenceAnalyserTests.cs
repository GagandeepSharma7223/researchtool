using chapterone.shared.utils;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace chapterone.logic.tests
{

    [Trait("ChangeAnalyser", "")]
    public class DifferenceAnalyserTests
    {
        [Fact]
        public void NullAndEmptyLists()
        {
            //
            // Test with null lists
            //

            // Run
            var changes = GetProcessedChanges<long>(null, null);

            // Check
            Assert.Empty(changes.Added);
            Assert.Empty(changes.Removed);

            //
            // Test with empty lists
            //

            // Run
            changes = GetProcessedChanges(GetListWith<long>(), GetListWith<long>());

            // Check
            Assert.Empty(changes.Added);
            Assert.Empty(changes.Removed);
        }

        [Fact]
        public void AddingOneFriend()
        {
            // Setup
            var newIds = GetListWith<long>(1234);
            var currentIds = GetListWith<long>();

            // Run
            var changes = GetProcessedChanges(newIds, currentIds);

            // Check
            Assert.Single(changes.Added);
            Assert.Empty(changes.Removed);
        }

        [Fact]
        public void RemovingOneFriend()
        {
            // Setup
            var newIds = GetListWith<long>();
            var currentIds = GetListWith<long>(1234);

            // Run
            var changes = GetProcessedChanges(newIds, currentIds);

            // Check
            Assert.Empty(changes.Added);
            Assert.Single(changes.Removed);
        }

        [Fact]
        public void AddingManyFriends()
        {
            // Setup
            var newIds = GetListWith(123, 234, 345, 456, 567);
            var currentIds = GetListWith(123, 234, 456);

            // Run
            var changes = GetProcessedChanges(newIds, currentIds);

            // Check
            Assert.Equal(2, changes.Added.Count());
            Assert.Empty(changes.Removed);
        }

        [Fact]
        public void RemovingManyFriends()
        {
            // Setup
            var newIds = GetListWith("123", "234", "456");
            var currentIds = GetListWith("123", "234", "345", "456", "567");

            // Run
            var changes = GetProcessedChanges(newIds, currentIds);

            // Check
            Assert.Empty(changes.Added);
            Assert.Equal(2, changes.Removed.Count());
        }


        [Fact]
        public void AddingRemovingManyFriends()
        {
            // Setup
            var newIds = GetListWith("123", "345", "567", "678");
            var currentIds = GetListWith("123", "234", "456", "678");

            // Run
            var changes = GetProcessedChanges(newIds, currentIds);

            // Check
            Assert.Equal(2, changes.Added.Count());
            Assert.Equal(2, changes.Removed.Count());
        }

        private Difference<T> GetProcessedChanges<T>(IList<T> newIds, IList<T> currentIds)
        {
            return DifferenceAnalyser<T>.ProcessChanges(newIds, currentIds);
        }

        private IList<T> GetListWith<T>(params T[] ids)
        {
            return new List<T>(ids);
        }
    }
}
