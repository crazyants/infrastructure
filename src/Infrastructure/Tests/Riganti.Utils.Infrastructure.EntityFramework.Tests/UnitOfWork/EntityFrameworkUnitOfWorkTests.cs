﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Riganti.Utils.Infrastructure.Core;
using Xunit;

#if EFCORE
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
#endif

#if EFCORE
namespace Riganti.Utils.Infrastructure.EntityFrameworkCore.Tests.UnitOfWork
#else
namespace Riganti.Utils.Infrastructure.EntityFramework.Tests.UnitOfWork
#endif
{
    public class EntityFrameworkUnitOfWorkTests
    {
        [Fact]
        public void Commit_CallCommitCoreOnlyIfHasOwnDbContext()
        {
            Func<DbContext> dbContextFactory = () => new Mock<DbContext>().Object;

            var unitOfWorkRegistryStub = new ThreadLocalUnitOfWorkRegistry();

            var unitOfWorkProvider = new EntityFrameworkUnitOfWorkProvider(unitOfWorkRegistryStub, dbContextFactory);

            var unitOfWorkParentMock = new Mock<EntityFrameworkUnitOfWork>(unitOfWorkProvider, dbContextFactory, DbContextOptions.ReuseParentContext) { CallBase = true };
            using (var unitOfWorkParent = unitOfWorkParentMock.Object)
            {
                unitOfWorkRegistryStub.RegisterUnitOfWork(unitOfWorkParent);

                var unitOfWorkChildMock = new Mock<EntityFrameworkUnitOfWork>(unitOfWorkProvider, dbContextFactory, DbContextOptions.ReuseParentContext) { CallBase = true };
                using (var unitOfWorkChild = unitOfWorkChildMock.Object)
                {
                    unitOfWorkChild.Commit();
                }
                unitOfWorkChildMock.Protected().Verify("CommitCore", Times.Never());

                unitOfWorkParent.Commit();
            }
            unitOfWorkParentMock.Protected().Verify("CommitCore", Times.Once());
        }

        [Fact]
        public void Commit_UOWHasNotParrent_CallCommitCore()
        {
            Func<DbContext> dbContextFactory = () => new Mock<DbContext>().Object;

            var unitOfWorkRegistryStub = new ThreadLocalUnitOfWorkRegistry();

            var unitOfWorkProvider = new EntityFrameworkUnitOfWorkProvider(unitOfWorkRegistryStub, dbContextFactory);

            var unitOfWorkParentMock = new Mock<EntityFrameworkUnitOfWork>(unitOfWorkProvider, dbContextFactory, DbContextOptions.ReuseParentContext) { CallBase = true };
            using (var unitOfWorkParent = unitOfWorkParentMock.Object)
            {
                unitOfWorkParent.Commit();
            }
            unitOfWorkParentMock.Protected().Verify("CommitCore", Times.Once());
        }

        [Fact]
        public void CommitAsync_UOWHasChild_CallCommitCore()
        {
            Func<DbContext> dbContextFactory = () => new Mock<DbContext>().Object;

            var unitOfWorkRegistryStub = new ThreadLocalUnitOfWorkRegistry();

            var unitOfWorkProvider = new EntityFrameworkUnitOfWorkProvider(unitOfWorkRegistryStub, dbContextFactory);

            var unitOfWorkParentMock = new Mock<EntityFrameworkUnitOfWork>(unitOfWorkProvider, dbContextFactory, DbContextOptions.ReuseParentContext) { CallBase = true };
            using (var unitOfWorkParent = unitOfWorkParentMock.Object)
            {
                unitOfWorkRegistryStub.RegisterUnitOfWork(unitOfWorkParent);

                using (var unitOfWorkChild = new EntityFrameworkUnitOfWork(unitOfWorkProvider, dbContextFactory, DbContextOptions.ReuseParentContext))
                {
                    unitOfWorkChild.Commit();
                }

                unitOfWorkParent.Commit();
            }
            unitOfWorkParentMock.Protected().Verify("CommitCore", Times.Once());
        }

        [Fact]
        public async Task CommitAsync_CallCommitCoreOnlyIfHasOwnDbContext()
        {
            Func<DbContext> dbContextFactory = () => new Mock<DbContext>().Object;

            var unitOfWorkRegistryStub = new ThreadLocalUnitOfWorkRegistry();

            var unitOfWorkProvider = new EntityFrameworkUnitOfWorkProvider(unitOfWorkRegistryStub, dbContextFactory);

            var unitOfWorkParentMock = new Mock<EntityFrameworkUnitOfWork>(unitOfWorkProvider, dbContextFactory, DbContextOptions.ReuseParentContext) { CallBase = true };
            using (var unitOfWorkParent = unitOfWorkParentMock.Object)
            {
                unitOfWorkRegistryStub.RegisterUnitOfWork(unitOfWorkParent);

                var unitOfWorkChildMock = new Mock<EntityFrameworkUnitOfWork>(unitOfWorkProvider, dbContextFactory, DbContextOptions.ReuseParentContext) { CallBase = true };
                using (var unitOfWorkChild = unitOfWorkChildMock.Object)
                {
                    await unitOfWorkChild.CommitAsync();
                }
                unitOfWorkChildMock.Protected().Verify("CommitAsyncCore", Times.Never(), new CancellationToken());

                await unitOfWorkParent.CommitAsync();
            }
            unitOfWorkParentMock.Protected().Verify("CommitAsyncCore", Times.Once(), new CancellationToken());
        }

        [Fact]
        public async Task CommitAsync_UOWHasNotParrent_CallCommitCore()
        {
            Func<DbContext> dbContextFactory = () => new Mock<DbContext>().Object;

            var unitOfWorkRegistryStub = new ThreadLocalUnitOfWorkRegistry();

            var unitOfWorkProvider = new EntityFrameworkUnitOfWorkProvider(unitOfWorkRegistryStub, dbContextFactory);

            var unitOfWorkParentMock = new Mock<EntityFrameworkUnitOfWork>(unitOfWorkProvider, dbContextFactory, DbContextOptions.ReuseParentContext) { CallBase = true };
            using (var unitOfWorkParent = unitOfWorkParentMock.Object)
            {
                await unitOfWorkParent.CommitAsync();
            }
            unitOfWorkParentMock.Protected().Verify("CommitAsyncCore", Times.Once(), new CancellationToken());
        }

        [Fact]
        public async Task Commit_UOWHasChild_CallCommitCore()
        {
            Func<DbContext> dbContextFactory = () => new Mock<DbContext>().Object;

            var unitOfWorkRegistryStub = new ThreadLocalUnitOfWorkRegistry();

            var unitOfWorkProvider = new EntityFrameworkUnitOfWorkProvider(unitOfWorkRegistryStub, dbContextFactory);

            var unitOfWorkParentMock = new Mock<EntityFrameworkUnitOfWork>(unitOfWorkProvider, dbContextFactory, DbContextOptions.ReuseParentContext) { CallBase = true };
            using (var unitOfWorkParent = unitOfWorkParentMock.Object)
            {
                unitOfWorkRegistryStub.RegisterUnitOfWork(unitOfWorkParent);

                using (var unitOfWorkChild = new EntityFrameworkUnitOfWork(unitOfWorkProvider, dbContextFactory, DbContextOptions.ReuseParentContext))
                {
                    await unitOfWorkChild.CommitAsync();
                }

                await unitOfWorkParent.CommitAsync();
            }
            unitOfWorkParentMock.Protected().Verify("CommitAsyncCore", Times.Once(), new CancellationToken());
        }
        
        [Fact]
        public void TryGetDbContext_UnitOfWorkRegistryHasUnitOfWork_ReturnCorrectDbContext()
        {
            var dbContext = new Mock<DbContext>().Object;
            Func<DbContext> dbContextFactory = () => dbContext;
            var unitOfWorkRegistryStub = new ThreadLocalUnitOfWorkRegistry();
            var unitOfWorkProvider = new EntityFrameworkUnitOfWorkProvider(unitOfWorkRegistryStub, dbContextFactory);
            var unitOfWork = new EntityFrameworkUnitOfWork(unitOfWorkProvider, dbContextFactory, DbContextOptions.ReuseParentContext);
            unitOfWorkRegistryStub.RegisterUnitOfWork(unitOfWork);

            var uowDbContext = EntityFrameworkUnitOfWork.TryGetDbContext(unitOfWorkProvider);

            Assert.NotNull(uowDbContext);
            Assert.Same(dbContext, uowDbContext);
        }

        [Fact]
        public void TryGetDbContext_UnitOfWorkRegistryHasNotUnitOfWork_ReturnsNull()
        {
            var dbContext = new Mock<DbContext>().Object;
            Func<DbContext> dbContextFactory = () => dbContext;
            var unitOfWorkRegistryStub = new ThreadLocalUnitOfWorkRegistry();
            var unitOfWorkProvider = new EntityFrameworkUnitOfWorkProvider(unitOfWorkRegistryStub, dbContextFactory);

            var value = EntityFrameworkUnitOfWork.TryGetDbContext(unitOfWorkProvider);
            Assert.Null(value);
        }
    }
}