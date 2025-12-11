import { useState, useEffect } from 'react';
import { BrowserRouter, Routes, Route, Navigate, Link } from 'react-router-dom';
import Login from './Login';
import Users from './Users';
import Roles from './Roles';
import GameManagement from './GameManagement';
import { hasAnyPermission } from './authUtils';

function App() {
  const [isAuth, setIsAuth] = useState(false);
  const [checking, setChecking] = useState(true);
  const [hasGameAccess, setHasGameAccess] = useState(false);

  useEffect(() => {
    const token = localStorage.getItem('token');
    setIsAuth(!!token);
    if (token) {
      const hasAccess = hasAnyPermission(['gm.games.read', 'gm.module.access']);
      console.log('Game Management access check:', hasAccess);
      setHasGameAccess(hasAccess);
    }
    setChecking(false);
  }, []);

  const handleLogin = () => {
    setIsAuth(true);
    const hasAccess = hasAnyPermission(['gm.games.read', 'gm.module.access']);
    setHasGameAccess(hasAccess);
  };

  const handleLogout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    setIsAuth(false);
    setHasGameAccess(false);
  };

  if (checking) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '100vh' }}>
        <div style={{ fontSize: '1.25rem', color: '#666' }}>Loading...</div>
      </div>
    );
  }

  return (
    <BrowserRouter>
      {isAuth ? (
        <div style={{ minHeight: '100vh', backgroundColor: '#f5f5f5', display: 'flex', flexDirection: 'column' }}>
          <nav style={{ backgroundColor: '#343a40', color: 'white', boxShadow: '0 2px 4px rgba(0,0,0,0.1)' }}>
            <div style={{ maxWidth: '1400px', margin: '0 auto', padding: '0 1rem' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', height: '4rem' }}>
                <div style={{ fontSize: '1.25rem', fontWeight: 'bold' }}>Admin Panel</div>
                <div style={{ display: 'flex', gap: '1.5rem' }}>
                  <Link
                    to="/users"
                    style={{
                      padding: '0.5rem 0.75rem',
                      borderRadius: '4px',
                      fontSize: '0.875rem',
                      fontWeight: '500',
                      textDecoration: 'none',
                      color: 'white'
                    }}
                  >
                    Users
                  </Link>
                  <Link
                    to="/roles"
                    style={{
                      padding: '0.5rem 0.75rem',
                      borderRadius: '4px',
                      fontSize: '0.875rem',
                      fontWeight: '500',
                      textDecoration: 'none',
                      color: 'white'
                    }}
                  >
                    Roles
                  </Link>
                  {hasGameAccess && (
                    <Link
                      to="/games"
                      style={{
                        padding: '0.5rem 0.75rem',
                        borderRadius: '4px',
                        fontSize: '0.875rem',
                        fontWeight: '500',
                        textDecoration: 'none',
                        color: 'white'
                      }}
                    >
                      Game Management
                    </Link>
                  )}
                </div>
                <button
                  onClick={handleLogout}
                  style={{
                    backgroundColor: '#dc3545',
                    padding: '0.5rem 1rem',
                    borderRadius: '4px',
                    fontSize: '0.875rem',
                    fontWeight: '500',
                    border: 'none',
                    color: 'white',
                    cursor: 'pointer'
                  }}
                >
                  Logout
                </button>
              </div>
            </div>
          </nav>
          <main style={{ flex: 1 }}>
            <Routes>
              <Route path="/" element={<Navigate to="/users" replace />} />
              <Route path="/users" element={<Users />} />
              <Route path="/roles" element={<Roles />} />
              {hasGameAccess ? (
                <Route path="/games" element={<GameManagement />} />
              ) : (
                <Route path="/games" element={
                  <div style={{ padding: '2rem', textAlign: 'center' }}>
                    <div style={{ 
                      backgroundColor: '#fee2e2', 
                      color: '#b91c1c', 
                      padding: '2rem', 
                      borderRadius: '8px',
                      maxWidth: '600px',
                      margin: '0 auto'
                    }}>
                      <h2 style={{ fontSize: '1.5rem', fontWeight: 'bold', marginBottom: '1rem' }}>Access Denied</h2>
                      <p>You do not have permission to access Game Management.</p>
                      <p style={{ marginTop: '0.5rem', fontSize: '0.9rem', color: '#991b1b' }}>
                        Required permission: gm.games.read or gm.module.access
                      </p>
                    </div>
                  </div>
                } />
              )}
            </Routes>
          </main>
        </div>
      ) : (
        <Login onLogin={handleLogin} />
      )}
    </BrowserRouter>
  );
}

export default App;
