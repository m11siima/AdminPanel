import { useState, useEffect } from 'react';
import { rolesApi, permissionsApi } from './api';
import type { Role, Permission, CreateRoleRequest, UpdateRolePermissionsRequest } from './api';

export default function Roles() {
  const [roles, setRoles] = useState<Role[]>([]);
  const [permissions, setPermissions] = useState<Permission[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editingRole, setEditingRole] = useState<Role | null>(null);
  const [roleName, setRoleName] = useState('');
  const [roleDescription, setRoleDescription] = useState('');
  const [selectedPermissions, setSelectedPermissions] = useState<string[]>([]);

  useEffect(() => {
    loadRoles();
    loadPermissions();
  }, []);

  const loadRoles = async () => {
    try {
      const data = await rolesApi.getAll();
      setRoles(data);
      setLoading(false);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error loading roles');
      setLoading(false);
    }
  };

  const loadPermissions = async () => {
    try {
      const data = await permissionsApi.getAll();
      setPermissions(data);
    } catch (err: any) {
      console.error('Error loading permissions:', err);
    }
  };

  const handleCreate = () => {
    setEditingRole(null);
    setRoleName('');
    setRoleDescription('');
    setSelectedPermissions([]);
    setShowModal(true);
  };

  const handleEdit = (role: Role) => {
    setEditingRole(role);
    setRoleName(role.name);
    setRoleDescription(role.description || '');
    setSelectedPermissions(role.permissionKeys);
    setShowModal(true);
  };

  const handleSave = async () => {
    try {
      if (editingRole) {
        const updateData: UpdateRolePermissionsRequest = {
          permissionKeys: selectedPermissions,
        };
        await rolesApi.updatePermissions(editingRole.id, updateData);
      } else {
        const createData: CreateRoleRequest = {
          name: roleName,
          description: roleDescription || undefined,
          permissionKeys: selectedPermissions,
        };
        await rolesApi.create(createData);
      }
      setShowModal(false);
      loadRoles();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error saving role');
    }
  };

  const togglePermission = (permissionKey: string) => {
    if (selectedPermissions.includes(permissionKey)) {
      setSelectedPermissions(selectedPermissions.filter(key => key !== permissionKey));
    } else {
      setSelectedPermissions([...selectedPermissions, permissionKey]);
    }
  };

  // группируем permissions по resource
  const groupedPerms: { [key: string]: Permission[] } = {};
  permissions.forEach(perm => {
    if (!groupedPerms[perm.resource]) {
      groupedPerms[perm.resource] = [];
    }
    groupedPerms[perm.resource].push(perm);
  });

  if (loading) {
    return <div style={{ padding: '2rem', textAlign: 'center' }}>Loading...</div>;
  }

  return (
    <div style={{ padding: '2rem' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2rem' }}>
        <h1 style={{ fontSize: '2rem', fontWeight: 'bold' }}>Roles</h1>
        <button
          onClick={handleCreate}
          style={{
            backgroundColor: '#28a745',
            color: 'white',
            padding: '0.75rem 1.5rem',
            border: 'none',
            borderRadius: '4px',
            cursor: 'pointer',
            fontSize: '1rem'
          }}
        >
          Create Role
        </button>
      </div>

      {error && (
        <div style={{ padding: '1rem', backgroundColor: '#f8d7da', color: '#721c24', borderRadius: '4px', marginBottom: '1rem' }}>
          {error}
        </div>
      )}

      <table style={{ width: '100%', borderCollapse: 'collapse', backgroundColor: 'white', boxShadow: '0 2px 4px rgba(0,0,0,0.1)' }}>
        <thead>
          <tr style={{ backgroundColor: '#f8f9fa' }}>
            <th style={{ padding: '1rem', textAlign: 'left', borderBottom: '1px solid #ddd' }}>Name</th>
            <th style={{ padding: '1rem', textAlign: 'left', borderBottom: '1px solid #ddd' }}>Description</th>
            <th style={{ padding: '1rem', textAlign: 'left', borderBottom: '1px solid #ddd' }}>System</th>
            <th style={{ padding: '1rem', textAlign: 'left', borderBottom: '1px solid #ddd' }}>Permissions</th>
            <th style={{ padding: '1rem', textAlign: 'left', borderBottom: '1px solid #ddd' }}>Actions</th>
          </tr>
        </thead>
        <tbody>
          {roles.map(role => (
            <tr key={role.id} style={{ borderBottom: '1px solid #ddd' }}>
              <td style={{ padding: '1rem', fontWeight: '500' }}>{role.name}</td>
              <td style={{ padding: '1rem' }}>{role.description || '-'}</td>
              <td style={{ padding: '1rem' }}>{role.isSystem ? 'Yes' : 'No'}</td>
              <td style={{ padding: '1rem' }}>{role.permissionKeys.length} permissions</td>
              <td style={{ padding: '1rem' }}>
                {!role.isSystem && (
                  <button
                    onClick={() => handleEdit(role)}
                    style={{
                      backgroundColor: '#007bff',
                      color: 'white',
                      padding: '0.5rem 1rem',
                      border: 'none',
                      borderRadius: '4px',
                      cursor: 'pointer'
                    }}
                  >
                    Edit
                  </button>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      {showModal && (
        <div
          style={{
            position: 'fixed',
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            backgroundColor: 'rgba(0,0,0,0.5)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            zIndex: 1000
          }}
          onClick={() => setShowModal(false)}
        >
          <div
            style={{
              backgroundColor: 'white',
              padding: '2rem',
              borderRadius: '8px',
              width: '90%',
              maxWidth: '700px',
              maxHeight: '90vh',
              overflowY: 'auto'
            }}
            onClick={(e) => e.stopPropagation()}
          >
            <h2 style={{ fontSize: '1.5rem', fontWeight: 'bold', marginBottom: '1rem' }}>
              {editingRole ? 'Edit Role' : 'Create Role'}
            </h2>
            {!editingRole && (
              <>
                <div style={{ marginBottom: '1rem' }}>
                  <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500' }}>Name:</label>
                  <input
                    type="text"
                    value={roleName}
                    onChange={(e) => setRoleName(e.target.value)}
                    required
                    maxLength={128}
                    style={{
                      width: '100%',
                      padding: '0.75rem',
                      border: '1px solid #ddd',
                      borderRadius: '4px',
                      fontSize: '1rem',
                      boxSizing: 'border-box'
                    }}
                  />
                </div>
                <div style={{ marginBottom: '1rem' }}>
                  <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500' }}>Description:</label>
                  <textarea
                    value={roleDescription}
                    onChange={(e) => setRoleDescription(e.target.value)}
                    maxLength={512}
                    rows={3}
                    style={{
                      width: '100%',
                      padding: '0.75rem',
                      border: '1px solid #ddd',
                      borderRadius: '4px',
                      fontSize: '1rem',
                      boxSizing: 'border-box',
                      fontFamily: 'inherit'
                    }}
                  />
                </div>
              </>
            )}
            {editingRole && (
              <div style={{ marginBottom: '1rem' }}>
                <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500' }}>Role: {editingRole.name}</label>
                <p style={{ fontSize: '0.9rem', color: '#666', marginTop: '0.5rem' }}>System roles cannot be edited</p>
              </div>
            )}
            <div style={{ marginBottom: '1rem' }}>
              <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500' }}>Permissions:</label>
              <div style={{ border: '1px solid #ddd', borderRadius: '4px', padding: '1rem', maxHeight: '400px', overflowY: 'auto' }}>
                {Object.keys(groupedPerms).map(resource => (
                  <div key={resource} style={{ marginBottom: '1.5rem' }}>
                    <h4 style={{ fontWeight: '600', marginBottom: '0.5rem', textTransform: 'capitalize' }}>{resource}</h4>
                    <div>
                      {groupedPerms[resource].map(perm => (
                        <label key={perm.key} style={{ display: 'flex', alignItems: 'center', marginBottom: '0.5rem', cursor: 'pointer' }}>
                          <input
                            type="checkbox"
                            checked={selectedPermissions.includes(perm.key)}
                            onChange={() => togglePermission(perm.key)}
                            style={{ marginRight: '0.5rem' }}
                          />
                          <span style={{ fontSize: '0.9rem' }}>
                            {perm.action} {perm.description && `(${perm.description})`}
                          </span>
                        </label>
                      ))}
                    </div>
                  </div>
                ))}
              </div>
            </div>
            <div style={{ display: 'flex', gap: '1rem' }}>
              <button
                onClick={handleSave}
                style={{
                  flex: 1,
                  backgroundColor: '#007bff',
                  color: 'white',
                  padding: '0.75rem',
                  border: 'none',
                  borderRadius: '4px',
                  cursor: 'pointer',
                  fontSize: '1rem',
                  fontWeight: '500'
                }}
              >
                Save
              </button>
              <button
                onClick={() => setShowModal(false)}
                style={{
                  flex: 1,
                  backgroundColor: '#6c757d',
                  color: 'white',
                  padding: '0.75rem',
                  border: 'none',
                  borderRadius: '4px',
                  cursor: 'pointer',
                  fontSize: '1rem',
                  fontWeight: '500'
                }}
              >
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
