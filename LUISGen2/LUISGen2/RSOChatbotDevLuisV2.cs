using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;

#pragma warning disable SA1137

namespace LUIS
{
    public enum Intent
    {
        Accessibility,
        AreYouReal,
        AskingQuestion,
        Cancel,
        DontKnow,
        JoinMember,
        MemberInfo,
        None,
        NotHave,
        NotMember,
        OptOut,
        Restart,
        ServiceRequest,
        StatusCheck,
        Vehicle,
    }

    public class IntentType
    {
        public Intent intent { get; set; }
        public float score { get; set; }
    }

    public enum Entity
    {
        accident,
        alternation,
        appreciation,
        contact,
            toAgent,
            toMe,
        datetimeV2,
        dimension,
        FiveWOneH,
        generalInfo,
            annualFee,
            benefit,
            card,
            expired,
            insurance,
            membership,
            membershipId,
            phone,
        geographyV2,
        greeting,
        help,
        howDeterminer,
        location,
            towDestination,
            vehicleLocation,
        memberId,
        membershipType,
        money,
        parkingLocation,
            freeAccess,
            restrictedAccess,
        payment,
            billing,
            makingPayment,
            reimbursement,
        personName,
        phoneNumber,
        question,
        rsoInfo,
            riContactPhoneNumber,
            riCurrentLocation,
            riMemberName,
            riMembershipId,
            riMemberZipCode,
            riRequestType,
            riTowingDestination,
            riVehicle,
            riVehicleAccess,
        rsoOtherInfo,
            charge,
            coverage,
            rentcar,
            roadsideService,
        schedule,
        serviceType,
            battery,
            fuel,
            lockout,
            tire,
            tow,
        signUp,
            renew,
        simpleAnswer,
            no,
            yes,
        tempMemberId,
        vehicleInfo,
            color,
            make,
            model,
            year,
        vehicleType,
            motorCycle,
            RV,
            trailer,
        zipCode,
    }

    public abstract class EntityTypeBase
    {
        public Entity type { get; set; }
        public string value { get; set; }
    }

    public class SimpleEntityType : EntityTypeBase
    {
    }

    public class RoleEntityType : EntityTypeBase
    {
        public Entity roleType { get; set; }
    }

    public class CompositeEntityType : EntityTypeBase
    {
        public Dictionary<Entity, object> entities = new Dictionary<Entity, object>();
    }

    public class Sentiment
    {
        public enum Label
        {
            Positive,
            Negative,
            Neutral,
        }

        public Label label { get; set; }
        public float score { get; set; }
    }

    public class LUISModel
    {
        public string utterance { get; set; }
        public IntentType topIntent { get; set; }
        public Dictionary<Entity, object> entities = new Dictionary<Entity, object>();
        public Sentiment sentiment { get; set; }

        public T[] entity<T>(Entity type)
        {
            foreach (KeyValuePair<Entity, object> keyValue in entities)
            {
                if (keyValue.Key == type)
                {
                    return (T[])keyValue.Value;
                }

                if (keyValue.Value.GetType().Equals(typeof(RoleEntityType[])))
                {
                    RoleEntityType[] roleEntityTypes = keyValue.Value as RoleEntityType[];
                    foreach (RoleEntityType roleEntityType in roleEntityTypes)
                    {
                        if (roleEntityType.roleType == type)
                        {
                            return (T[])Convert.ChangeType(roleEntityTypes, typeof(T[]));
                        }
                    }
                }
            }

            return default(T[]);
        }
    }
}
