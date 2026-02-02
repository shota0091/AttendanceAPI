using Attendance.DTOs;
using Attendance.Models;
using Attendance.Repositories;
using Attendance.Services;
using Microsoft.Extensions.Configuration; // ★これを追加！
using Moq;
using Xunit;

namespace Attendance.Tests
{
    public class AttendanceServiceTests
    {
        // CheckInテスト：正常系
        [Fact]
        public void ClockIn_ShouldReturnRecord_WhenNotClockedInToday()
        {
            // ■ Arrange
            var mockRepo = new Mock<IAttendanceRepository>();

            // このテスト固有の設定（今日はまだ出勤してない）
            mockRepo.Setup(repo => repo.HasClockInToday(It.IsAny<string>(), It.IsAny<DateTime>()))
                    .Returns(false);

            // ★ここがスッキリ！
            // 共通メソッドを呼ぶだけで Service が手に入る
            var service = CreateService(mockRepo);

            var request = new UserRequest { UserId = "TestUser" };

            // ■ Act
            var result = service.ClockIn(request);

            // ■ Assert (検証)
            Assert.NotNull(result);
            Assert.Equal("TestUser", result.UserId);
            Assert.Equal(0, result.ApprovalStatus);

            mockRepo.Verify(repo => repo.Add(It.IsAny<AttendanceRecord>()), Times.Once);
        }

        // CheckOutテスト：正常系
        [Fact]
        public void ClockOut_ShouldUpdateRecord_WhenClockedIn()
        {
            // ■ Arrange
            var mockRepo = new Mock<IAttendanceRepository>();

            // ★ここが重要！
            // 「最新のレコードを頂戴」と言われたら、「出勤中のデータ」を渡すように仕込む
            var existingRecord = new AttendanceRecord
            {
                UserId = "TestUser",
                WorkDate = DateTime.Today,
                ClockInTime = DateTime.Now.AddHours(-9), // 9時間前に出勤したことにする
                ClockOutTime = null // まだ退勤していない
            };

            // GetLastActiveRecord が呼ばれたら、この existingRecord を返す！
            mockRepo.Setup(repo => repo.GetLastActiveRecord("TestUser"))
                    .Returns(existingRecord);

            // ★ここがスッキリ！
            // 共通メソッドを呼ぶだけで Service が手に入る
            var service = CreateService(mockRepo);
            var request = new UserRequest { UserId = "TestUser" };

            // ■ Act (実行)
            var result = service.ClockOut(request);

            // ■ Assert (検証)
            Assert.NotNull(result);
            Assert.NotNull(result.ClockOutTime); // 退勤時間が入っていること
            Assert.Equal("TestUser", result.UserId);

            // 最後に「Update（上書き保存）」が呼ばれたかチェック
            // ※リポジトリに Update メソッドがある前提です。もし SaveChanges だけなら書き換えてください
            mockRepo.Verify(repo => repo.Update(It.IsAny<AttendanceRecord>()), Times.Once);
        }

        // 月次情報の取得
        [Fact]
        public void GetMonthlyHistory_ShouldReturnList_WhenRecordsExist()
        {
            // ■ Arrange
            var mockRepo = new Mock<IAttendanceRepository>();

            // 1. 返却用の「リスト」を作る
            var fakeList = new List<AttendanceRecord>
            {
                new AttendanceRecord
                {
                    UserId = "TestUser",
                    WorkDate = new DateTime(2026, 2, 1),
                    ClockInTime = new DateTime(2026, 2, 1, 9, 0, 0),
                    ClockOutTime = new DateTime(2026, 2, 1, 18, 0, 0)
                }
            };

            // 2. ★ここが修正ポイント！
            // 「GetMonthlyHistory」の中で呼ばれているリポジトリのメソッド（例: GetByMonth）をモックする
            // ※ repo.GetByMonth の部分は、実際にあなたが作ったメソッド名に変えてください
            mockRepo.Setup(repo => repo.GetMonthlyHistory("TestUser", 2026, 2))
                    .Returns(fakeList);

            var service = CreateService(mockRepo);

            // ■ Act
            var result = service.GetMonthlyHistory("TestUser", 2026, 2);

            // ■ Assert
            Assert.NotNull(result);
            Assert.Single(result); // 「データが1件だけあること」を確認

            // 中身も合ってるか確認
            var firstRecord = result.First();
            Assert.Equal("TestUser", firstRecord.UserId);
            Assert.Equal(new DateTime(2026, 2, 1), firstRecord.WorkDate);
        }

        // CheckInテスト：異常系
        [Fact]
        public void ClockIn_ShouldThrowException_WhenAlreadyClockedIn()
        {
            // ■ Arrange
            var mockRepo = new Mock<IAttendanceRepository>();

            // このテスト固有の設定（今日はまだ出勤してない）
            mockRepo.Setup(repo => repo.HasClockInToday(It.IsAny<string>(), It.IsAny<DateTime>()))
                    .Returns(true);

            // ★ここがスッキリ！
            // 共通メソッドを呼ぶだけで Service が手に入る
            var service = CreateService(mockRepo);
            var request = new UserRequest { UserId = "TestUser" };

            // ■ Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => service.ClockIn(request));

            Assert.Equal("本日は既に出勤打刻済みです。", ex.Message);
        }

        // CheckOutテスト：異常系（出勤してないのに退勤しようとした）
        [Fact]
        public void ClockOut_ShouldThrowException_WhenNotClockedIn()
        {
            // ■ Arrange
            var mockRepo = new Mock<IAttendanceRepository>();

            // 「最新のデータ頂戴」と言われたら「null（ないよ）」と答える
            mockRepo.Setup(repo => repo.GetLastActiveRecord(It.IsAny<string>()))
                    .Returns((AttendanceRecord)null);

            var service = CreateService(mockRepo);
            var request = new UserRequest { UserId = "TestUser" };

            // ■ Act & Assert
            // InvalidOperationException が出ることを期待する
            var ex = Assert.Throws<InvalidOperationException>(() => service.ClockOut(request));

            // エラーメッセージがあっているか確認
            // ※Serviceの実装メッセージに合わせて修正してください
            Assert.Equal("出勤データが見つかりません。先に打刻してください。", ex.Message);
        }


        private AttendanceService CreateService(Mock<IAttendanceRepository> mockRepo)
        {
            // 毎回書くのが面倒な設定ファイルのモック作り
            var mockConfig = new Mock<IConfiguration>();
            var mockSection = new Mock<IConfigurationSection>();
            mockSection.Setup(s => s.Value).Returns("5"); // 5時切り
            mockConfig.Setup(c => c.GetSection(It.IsAny<string>())).Returns(mockSection.Object);

            // 作ったConfigと、引数で受け取ったRepoを使ってServiceを返す
            return new AttendanceService(mockRepo.Object, mockConfig.Object);
        }

    }
}