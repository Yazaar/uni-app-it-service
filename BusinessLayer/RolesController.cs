using DataLayer;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer
{
    public class RolesController
    {
        public Roll Login(string username, string password)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                return unitOfWork.RoleRepository.Find(r => r.Användarnamn == username && r.Lösenord == password, r => r.RollBehörighet);
            }
        }

        public IEnumerable<RollBehörighet> GetAllRolePermissions()
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                return unitOfWork.RolePermissionRepository.FindAll();
            }
        }

        public IEnumerable<Roll> GetAllRoles()
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                return unitOfWork.RoleRepository.FindAll(r => r.RollBehörighet);
            }
        }

        public void DeleteRole(Roll role)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Roll roleDB = unitOfWork.RoleRepository.Find(r => r.RollID == role.RollID);
                if (roleDB is null) return;
                unitOfWork.RoleRepository.Remove(roleDB);
                unitOfWork.Save();
            }
        }

        public Roll CreateRole(string roleName, string roleUsername, string rolePassword, RollBehörighet rolePermission)
        {

            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Roll roleDB = unitOfWork.RoleRepository.Find(r => r.Användarnamn == roleUsername);
                if (roleDB is not null) return null;

                RollBehörighet rolePermissionDB = unitOfWork.RolePermissionRepository.Find(rp => rp.RollBehörighetID == rolePermission.RollBehörighetID);
                if (rolePermissionDB is null) return null;

                roleDB = new Roll()
                {
                    Användarnamn = roleUsername,
                    Benämning = roleName,
                    Lösenord = rolePassword,
                    RollBehörighet = rolePermissionDB
                };

                unitOfWork.RoleRepository.Add(roleDB);
                unitOfWork.Save();
                return roleDB;
            }
        }

        public Roll UpdateRole(Roll role, string roleName, string roleUsername, string rolePassword, RollBehörighet rolePermission)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                Roll roleDB = unitOfWork.RoleRepository.Find(r => r.Användarnamn == roleUsername);
                if (roleDB is not null && roleDB.RollID != role.RollID) return null;

                RollBehörighet rolePermissionDB = unitOfWork.RolePermissionRepository.Find(rp => rp.RollBehörighetID == rolePermission.RollBehörighetID);
                if (rolePermissionDB is null) return null;

                roleDB = unitOfWork.RoleRepository.Find(r => r.RollID == role.RollID, r => r.RollBehörighet);
                if (roleDB is null) return null;

                roleDB.Benämning = roleName;
                roleDB.Användarnamn = roleUsername;
                if (rolePassword.Length > 0) roleDB.Lösenord = rolePassword;
                roleDB.RollBehörighet = rolePermissionDB;

                unitOfWork.Save();
                return roleDB;
            }
        }
    }
}
