using Claims.Application.Abstractions;
using Claims.Application.DTOs;
using Claims.Application.Exceptions;
using Claims.Application.Services;
using Claims.Domain.Enums;
using Claims.Tests.Fakes;
using Xunit;

namespace Claims.Tests;

public class ClaimServiceTests
{
    private readonly ClaimService _service;
    private readonly IClaimRepository _repository;

    public ClaimServiceTests()
    {
        _repository = new InMemoryClaimRepository();
        _service = new ClaimService(_repository);
    }

    private static CreateClaimRequest ValidCreateRequest() => new()
    {
        PolicyNumber = "POL-TEST-1",
        InsuredName = "Test Asegurado",
        ClaimType = "Auto",
        EstimatedAmount = 15000m
    };

    [Fact]
    public async Task CreateClaim_WithInvalidAmount_ShouldFail()
    {
        var request = ValidCreateRequest() with { EstimatedAmount = 0 };

        await Assert.ThrowsAsync<DomainValidationException>(() => _service.CreateClaimAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateClaim_WithMissingPolicyNumber_ShouldFail()
    {
        var request = ValidCreateRequest() with { PolicyNumber = "   " };

        await Assert.ThrowsAsync<DomainValidationException>(() => _service.CreateClaimAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateClaim_WithMissingInsuredName_ShouldFail()
    {
        var request = ValidCreateRequest() with { InsuredName = string.Empty };

        await Assert.ThrowsAsync<DomainValidationException>(() => _service.CreateClaimAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateClaim_WithMissingClaimType_ShouldFail()
    {
        var request = ValidCreateRequest() with { ClaimType = string.Empty };

        await Assert.ThrowsAsync<DomainValidationException>(() => _service.CreateClaimAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateClaim_Valid_ShouldSucceedAndStartInOpen()
    {
        var created = await _service.CreateClaimAsync(ValidCreateRequest(), CancellationToken.None);

        Assert.True(created.Id > 0);
        Assert.Equal("OPEN", created.Status);
    }

    [Fact]
    public async Task GetClaim_NonExistent_ShouldThrowNotFound()
    {
        await Assert.ThrowsAsync<ClaimNotFoundException>(() => _service.GetClaimAsync(999, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateClaim_Valid_ShouldUpdateFields()
    {
        var created = await _service.CreateClaimAsync(ValidCreateRequest(), CancellationToken.None);

        var updated = await _service.UpdateClaimAsync(created.Id, new UpdateClaimRequest
        {
            PolicyNumber = "POL-TEST-2",
            InsuredName = "Otro Asegurado",
            ClaimType = "Hogar",
            EstimatedAmount = 25000m
        }, CancellationToken.None);

        Assert.Equal("POL-TEST-2", updated.PolicyNumber);
        Assert.Equal("Hogar", updated.ClaimType);
        Assert.Equal(25000m, updated.EstimatedAmount);
        Assert.Equal("OPEN", updated.Status);
    }

    [Fact]
    public async Task ChangeStatus_OpenToInReview_ShouldSucceed()
    {
        var created = await _service.CreateClaimAsync(ValidCreateRequest(), CancellationToken.None);

        var result = await _service.ChangeStatusAsync(created.Id, new ChangeClaimStatusRequest { NewStatus = ClaimStatus.IN_REVIEW }, CancellationToken.None);

        Assert.Equal("IN_REVIEW", result.Status);
    }

    [Fact]
    public async Task ChangeStatus_InReviewToClosed_ShouldSucceed()
    {
        var created = await _service.CreateClaimAsync(ValidCreateRequest(), CancellationToken.None);
        await _service.ChangeStatusAsync(created.Id, new ChangeClaimStatusRequest { NewStatus = ClaimStatus.IN_REVIEW }, CancellationToken.None);

        var result = await _service.ChangeStatusAsync(created.Id, new ChangeClaimStatusRequest { NewStatus = ClaimStatus.CLOSED }, CancellationToken.None);

        Assert.Equal("CLOSED", result.Status);
    }

    [Fact]
    public async Task ChangeStatus_ClosedToOpen_ShouldFail()
    {
        var created = await _service.CreateClaimAsync(ValidCreateRequest(), CancellationToken.None);
        await _service.ChangeStatusAsync(created.Id, new ChangeClaimStatusRequest { NewStatus = ClaimStatus.IN_REVIEW }, CancellationToken.None);
        await _service.ChangeStatusAsync(created.Id, new ChangeClaimStatusRequest { NewStatus = ClaimStatus.CLOSED }, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidStatusTransitionException>(() =>
            _service.ChangeStatusAsync(created.Id, new ChangeClaimStatusRequest { NewStatus = ClaimStatus.OPEN }, CancellationToken.None));
    }

    [Fact]
    public async Task ChangeStatus_InReviewToOpen_ShouldFail()
    {
        var created = await _service.CreateClaimAsync(ValidCreateRequest(), CancellationToken.None);
        await _service.ChangeStatusAsync(created.Id, new ChangeClaimStatusRequest { NewStatus = ClaimStatus.IN_REVIEW }, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidStatusTransitionException>(() =>
            _service.ChangeStatusAsync(created.Id, new ChangeClaimStatusRequest { NewStatus = ClaimStatus.OPEN }, CancellationToken.None));
    }

    [Fact]
    public async Task ChangeStatus_ClosedToInReview_ShouldFail()
    {
        var created = await _service.CreateClaimAsync(ValidCreateRequest(), CancellationToken.None);
        await _service.ChangeStatusAsync(created.Id, new ChangeClaimStatusRequest { NewStatus = ClaimStatus.IN_REVIEW }, CancellationToken.None);
        await _service.ChangeStatusAsync(created.Id, new ChangeClaimStatusRequest { NewStatus = ClaimStatus.CLOSED }, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidStatusTransitionException>(() =>
            _service.ChangeStatusAsync(created.Id, new ChangeClaimStatusRequest { NewStatus = ClaimStatus.IN_REVIEW }, CancellationToken.None));
    }

    [Fact]
    public async Task ChangeStatus_ShouldCreateHistory()
    {
        var created = await _service.CreateClaimAsync(ValidCreateRequest(), CancellationToken.None);

        await _service.ChangeStatusAsync(created.Id, new ChangeClaimStatusRequest { NewStatus = ClaimStatus.IN_REVIEW }, CancellationToken.None);
        var history = await _service.GetStatusHistoryAsync(created.Id, CancellationToken.None);

        Assert.Collection(history,
            first =>
            {
                Assert.Null(first.PreviousStatus);
                Assert.Equal("OPEN", first.NewStatus);
            },
            second =>
            {
                Assert.Equal("OPEN", second.PreviousStatus);
                Assert.Equal("IN_REVIEW", second.NewStatus);
            });
    }

    [Fact]
    public async Task GetClaims_WithStatusFilter_ShouldReturnFilteredResults()
    {
        await _service.CreateClaimAsync(ValidCreateRequest(), CancellationToken.None);
        var open = await _service.CreateClaimAsync(ValidCreateRequest() with { PolicyNumber = "POL-OPEN-2" }, CancellationToken.None);
        var review = await _service.CreateClaimAsync(ValidCreateRequest() with { PolicyNumber = "POL-REVIEW-1" }, CancellationToken.None);
        await _service.ChangeStatusAsync(review.Id, new ChangeClaimStatusRequest { NewStatus = ClaimStatus.IN_REVIEW }, CancellationToken.None);

        var onlyOpen = await _service.GetClaimsAsync(ClaimStatus.OPEN, null, CancellationToken.None);
        var onlyReview = await _service.GetClaimsAsync(ClaimStatus.IN_REVIEW, null, CancellationToken.None);

        Assert.Contains(onlyOpen, c => c.Id == open.Id);
        Assert.DoesNotContain(onlyOpen, c => c.Id == review.Id);
        Assert.Contains(onlyReview, c => c.Id == review.Id);
        Assert.DoesNotContain(onlyReview, c => c.Id == open.Id);
    }

    [Fact]
    public async Task GetClaims_WithPolicyNumberFilter_ShouldReturnFilteredResults()
    {
        var created = await _service.CreateClaimAsync(ValidCreateRequest(), CancellationToken.None);
        await _service.CreateClaimAsync(ValidCreateRequest() with { PolicyNumber = "POL-OTHER-1" }, CancellationToken.None);

        var result = await _service.GetClaimsAsync(null, "POL-TEST", CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(created.Id, result[0].Id);
    }

    [Fact]
    public async Task DeleteClaim_ShouldRemoveClaimAndHistory()
    {
        var created = await _service.CreateClaimAsync(ValidCreateRequest(), CancellationToken.None);
        await _service.ChangeStatusAsync(created.Id, new ChangeClaimStatusRequest { NewStatus = ClaimStatus.IN_REVIEW }, CancellationToken.None);

        await _service.DeleteClaimAsync(created.Id, CancellationToken.None);

        await Assert.ThrowsAsync<ClaimNotFoundException>(() => _service.GetClaimAsync(created.Id, CancellationToken.None));
    }
}