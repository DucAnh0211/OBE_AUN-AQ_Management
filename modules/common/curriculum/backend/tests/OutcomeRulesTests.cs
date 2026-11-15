using ObeAunQa.Modules.Curriculum.Application;
using ObeAunQa.Modules.Curriculum.Domain;
namespace ObeAunQa.Modules.Curriculum.Tests;

public sealed class OutcomeRulesTests
{
    private static OutcomeInput Input(bool missing = true)
    {
        var course = new OutcomeCourse(1, 100, "HP1", "Test", 3,
            [new(1, "CLO1", "One"), new(2, "CLO2", "Two")],
            [new(10, "A", 40, 10, new Dictionary<long, decimal>{{1,100}}), new(11, "B", 60, 10, new Dictionary<long, decimal>{{1,50},{2,50}})],
            [new(1,"X",[1,2])]);
        OutcomeAttempt Row(long id, decimal a, decimal? b) => new(id,id,$"S{id}","Student", "K1", 1,1,"2026-2027-HK1","published",1,null,6,2,true,true,
            new Dictionary<long,decimal?>{{10,a},{11,b}});
        return new(1,1,new(1),new(),[course],[new(1,"PLO1","Plo")],
            missing ? [Row(1,8,6),Row(2,4,2),Row(3,10,8),Row(4,6,null)] : [Row(1,8,6),Row(2,4,2),Row(3,10,8)]);
    }
    [Fact] public void GoldenCloFixtureAndCoverage()
    {
        var r = OutcomeRules.Calculate(Input());
        Assert.Equal(50m/7m, r.CloStudents.Single(s=>s.StudentId==1 && s.OutcomeId==1).Score);
        var c = r.Clos.Single(c=>c.OutcomeId==1);
        Assert.Equal(4,c.EligibleCount); Assert.Equal(3,c.EvaluatedCount); Assert.Equal(2,c.AchievedCount);
        Assert.Equal(1,c.MissingCount); Assert.Equal(75,c.DataCoverageRate); Assert.Equal(200m/3m,c.AchievementRate);
        Assert.Equal("partial",r.Status);
    }
    [Fact] public void ZeroIsNotMissingAndSpecialMarksAreNotAchievement()
    {
        var i=Input(false); var a=i.Attempts[0];
        i=i with { Attempts=[a with { Scores=new Dictionary<long,decimal?>{{10,0},{11,0}} },i.Attempts[1] with { SpecialMark="P" }] };
        var r=OutcomeRules.Calculate(i);
        Assert.Equal(0,r.CloStudents.First().Score); Assert.Equal("not_achieved",r.CloStudents.First().Status);
        Assert.All(r.CloStudents.Where(c=>c.StudentId==2), c=>Assert.Equal("excluded",c.Status));
    }
    [Fact] public void GoldenPloUsesCreditsAndDoesNotAverageClassPercentages()
    {
        var i=Input(false); var c=i.Courses[0];
        c=c with { Assessments=[new(10,"A",100,10,new Dictionary<long,decimal>{{1,50},{2,50}})] };
        var second=new OutcomeCourse(2,200,"HP2","Test2",2,[new(3,"CLO3","Three")],
            [new(12,"A",100,10,new Dictionary<long,decimal>{{3,100}})],[new(1,"Y",[3])]);
        var a=i.Attempts[0];
        i=i with { Courses=[c,second],Attempts=[a with { Scores=new Dictionary<long,decimal?>{{10,7}} }, a with { EnrollmentId=5,ProgramCourseId=2,OfferingId=2,Scores=new Dictionary<long,decimal?>{{12,4}} }] };
        Assert.Equal(6.25m,OutcomeRules.Calculate(i).PloStudents.Single().Score);
        var rows=Enumerable.Range(0,10).Select(n=>new StudentOutcome(n,"","",null,null,1,"P",n==0?2:8,5,n==0?"not_achieved":"achieved",null,[],[]));
        Assert.Equal(90,OutcomeRules.Aggregate(1,"P",rows,70).AchievementRate);
    }
    [Fact] public void InvalidMappingNoDataAndRoundingBoundary()
    {
        var i=Input(false);var course=i.Courses[0];
        var bad=course with {Assessments=[course.Assessments[0] with {Weight=100,Clos=new Dictionary<long,decimal>{{1,90}}}]};
        Assert.Contains(OutcomeRules.Calculate(i with {Courses=[bad]}).Issues,x=>x.Code=="assessment_mapping");
        Assert.Null(OutcomeRules.Aggregate(1,"P",[],70).AchievementRate);
        Assert.True(OutcomeRules.PeriodOrder("2026-2027-HKHE")>OutcomeRules.PeriodOrder("2026-2027-HK2"));
        var a=i.Attempts[0] with { Scores=new Dictionary<long,decimal?>{{10,4.999m},{11,4.999m}} };
        Assert.All(OutcomeRules.Calculate(i with {Attempts=[a]}).CloStudents,c=>Assert.Equal("not_achieved",c.Status));
    }
    [Fact] public void RetakesChooseOneWholeAttemptAndPolicyChangesHash()
    {
        var i=Input(false); var a=i.Attempts[0];
        var second=a with {EnrollmentId=99,OfferingId=99,Period="2026-2027-HK2",FinalScore=9,Scores=new Dictionary<long,decimal?>{{10,2},{11,2}}};
        var r=OutcomeRules.Calculate(i with {Attempts=[a,second]});
        Assert.Equal(new long[]{99},r.PloStudents.Single().EnrollmentIds);
        Assert.NotEqual(r.SourceHash,OutcomeRules.Calculate(i with {Policy=new(1)}).SourceHash);
    }
    [Fact] public void LatestQuantitativeAttemptIsSharedByCloAndPloAggregates()
    {
        var i=Input(false);var first=i.Attempts[0];
        var incomplete=first with {EnrollmentId=99,OfferingId=99,Period="2026-2027-HK2",SpecialMark="I",FinalScore=null};
        var result=OutcomeRules.Calculate(i with {Policy=new(RetakePolicy:"latest"),Attempts=[first,incomplete]});
        Assert.All(result.Clos,c=>Assert.Equal(1,c.EvaluatedCount));
        Assert.Equal(new long[]{first.EnrollmentId},result.PloStudents.Single().EnrollmentIds);
        Assert.Equal("official",result.Status);
    }
}
