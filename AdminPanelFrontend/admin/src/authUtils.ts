export function getPermissionsFromToken(): string[] {
  const token = localStorage.getItem('token');
  if (!token) {
    return [];
  }

  try {
    const parts = token.split('.');
    if (parts.length !== 3) {
      return [];
    }

    const payload = parts[1];
    const paddedPayload = payload + '='.repeat((4 - payload.length % 4) % 4);
    const decoded = atob(paddedPayload);
    const parsed = JSON.parse(decoded);

    const permissions: string[] = [];
    
    if (parsed.permission) {
      if (Array.isArray(parsed.permission)) {
        parsed.permission.forEach((perm: any) => {
          if (typeof perm === 'string' && !permissions.includes(perm)) {
            permissions.push(perm);
          }
        });
      } else if (typeof parsed.permission === 'string') {
        permissions.push(parsed.permission);
      }
    }
    
    if (parsed.Permission) {
      if (Array.isArray(parsed.Permission)) {
        parsed.Permission.forEach((perm: any) => {
          if (typeof perm === 'string' && !permissions.includes(perm)) {
            permissions.push(perm);
          }
        });
      } else if (typeof parsed.Permission === 'string' && !permissions.includes(parsed.Permission)) {
        permissions.push(parsed.Permission);
      }
    }
    
    Object.keys(parsed).forEach(key => {
      if (key.toLowerCase().includes('permission') && key !== 'permission' && key !== 'Permission') {
        const value = parsed[key];
        if (Array.isArray(value)) {
          value.forEach((perm: any) => {
            if (typeof perm === 'string' && !permissions.includes(perm)) {
              permissions.push(perm);
            }
          });
        } else if (typeof value === 'string' && !permissions.includes(value)) {
          permissions.push(value);
        }
      }
    });

    console.log('Full token payload:', parsed);
    console.log('All keys in payload:', Object.keys(parsed));
    console.log('Extracted permissions:', permissions);

    return permissions;
  } catch (err) {
    console.error('Error decoding token:', err);
    return [];
  }
}

export function isSuperAdmin(): boolean {
  const token = localStorage.getItem('token');
  if (!token) {
    return false;
  }

  try {
    const parts = token.split('.');
    if (parts.length !== 3) {
      return false;
    }

    const payload = parts[1];
    const paddedPayload = payload + '='.repeat((4 - payload.length % 4) % 4);
    const decoded = atob(paddedPayload);
    const parsed = JSON.parse(decoded);

    return parsed.is_superadmin === 'true' || parsed.is_superadmin === true;
  } catch (err) {
    console.error('Error decoding token:', err);
    return false;
  }
}

export function hasPermission(permission: string): boolean {
  if (isSuperAdmin()) {
    return true;
  }

  const permissions = getPermissionsFromToken();
  return permissions.includes(permission);
}

export function hasAnyPermission(permissions: string[]): boolean {
  const isSuper = isSuperAdmin();
  if (isSuper) {
    console.log('User is SuperAdmin, granting access');
    return true;
  }

  const userPermissions = getPermissionsFromToken();
  console.log('Checking permissions:', permissions);
  console.log('User has permissions:', userPermissions);
  
  const hasAccess = permissions.some(perm => userPermissions.includes(perm));
  console.log('Has access result:', hasAccess);
  
  return hasAccess;
}

