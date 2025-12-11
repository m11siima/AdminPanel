import { useState, useEffect } from 'react';
import { gamesApi, configsApi, platformsApi } from './api';
import type { Game, Config, Platform, CreateConfigRequest, UpdateConfigRequest, CreatePlatformRequest, UpdatePlatformRequest, UpdateGameRequest, GameConfigItem } from './api';
import { hasAnyPermission } from './authUtils';

export default function GameManagement() {
  const [tab, setTab] = useState('games');
  const [hasAccess, setHasAccess] = useState(false);
  const [games, setGames] = useState<Game[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [searchTitle, setSearchTitle] = useState('');
  const [searchPath, setSearchPath] = useState('');
  const [provider, setProvider] = useState('');
  const [category, setCategory] = useState('');
  const [sortBy, setSortBy] = useState('id');
  const [sortOrder, setSortOrder] = useState('asc');
  const [configs, setConfigs] = useState<Config[]>([]);
  const [platforms, setPlatforms] = useState<Platform[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showConfigModal, setShowConfigModal] = useState(false);
  const [showPlatformModal, setShowPlatformModal] = useState(false);
  const [editingConfig, setEditingConfig] = useState<Config | null>(null);
  const [editingPlatform, setEditingPlatform] = useState<Platform | null>(null);
  const [configName, setConfigName] = useState('');
  const [configGameIds, setConfigGameIds] = useState<number[]>([]);
  const [configGames, setConfigGames] = useState<Map<number, boolean>>(new Map());
  const [platformName, setPlatformName] = useState('');
  const [platformDescription, setPlatformDescription] = useState('');
  const [platformUrl, setPlatformUrl] = useState('');
  const [platformConfigId, setPlatformConfigId] = useState('');
  const [allGames, setAllGames] = useState<Game[]>([]);
  const [showGameModal, setShowGameModal] = useState(false);
  const [editingGame, setEditingGame] = useState<Game | null>(null);
  const [gameTitle, setGameTitle] = useState('');
  const [gamePath, setGamePath] = useState('');
  const [gameCategory, setGameCategory] = useState('');
  const [gameTags, setGameTags] = useState('');

  useEffect(() => {
    const access = hasAnyPermission(['gm.games.read', 'gm.module.access']);
    setHasAccess(access);
    if (!access) {
      setError('You do not have permission to access Game Management');
      setLoading(false);
      return;
    }

    if (tab === 'games') {
      loadGames();
    } else if (tab === 'configurations') {
      loadConfigs();
    } else if (tab === 'platforms') {
      loadPlatforms();
    }
  }, [tab, page, searchTitle, searchPath, provider, category, sortBy, sortOrder]);

  useEffect(() => {
    loadAllGamesForConfigs();
  }, []);

  const loadAllGamesForConfigs = async () => {
    try {
      const response = await gamesApi.getAll({ page: 1, pageSize: 10000 });
      setAllGames((response as any).games || (response as any).Games || []);
    } catch (err) {
      console.error('Error loading games:', err);
    }
  };

  const loadGames = async () => {
    try {
      setLoading(true);
      const response = await gamesApi.getAll({
        page: page,
        pageSize: 25,
        searchTitle: searchTitle || undefined,
        searchPath: searchPath || undefined,
        provider: provider || undefined,
        category: category || undefined,
        sortBy: sortBy,
        sortOrder: sortOrder,
      });
      setGames((response as any).games || (response as any).Games || []);
      setTotalCount(response.totalCount);
      setLoading(false);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error loading games');
      setLoading(false);
    }
  };

  const loadConfigs = async () => {
    try {
      setLoading(true);
      const data = await configsApi.getAll();
      setConfigs(data);
      setLoading(false);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error loading configs');
      setLoading(false);
    }
  };

  const loadPlatforms = async () => {
    try {
      setLoading(true);
      const data = await platformsApi.getAll();
      setPlatforms(data);
      setLoading(false);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error loading platforms');
      setLoading(false);
    }
  };

  const handleSearchClick = () => {
    setPage(1);
    loadGames();
  };

  const handleSort = (field: string) => {
    if (sortBy === field) {
      setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc');
    } else {
      setSortBy(field);
      setSortOrder('asc');
    }
    setPage(1);
  };

  const handleCreateConfig = () => {
    setEditingConfig(null);
    setConfigName('');
    setConfigGameIds([]);
    setConfigGames(new Map());
    setShowConfigModal(true);
  };

  const handleEditConfig = (config: Config) => {
    setEditingConfig(config);
    setConfigName(config.name);
    setConfigGameIds(config.gameIds);
    
    const gamesMap = new Map<number, boolean>();
    if (config.games && config.games.length > 0) {
      config.games.forEach(game => {
        gamesMap.set(game.gameId, game.isEnabled);
      });
    } else {
      config.gameIds.forEach(gameId => {
        gamesMap.set(gameId, true);
      });
    }
    setConfigGames(gamesMap);
    setShowConfigModal(true);
  };

  const handleSaveConfig = async () => {
    try {
      if (editingConfig) {
        const games: GameConfigItem[] = configGameIds.map(gameId => ({
          gameId,
          isEnabled: configGames.has(gameId) ? (configGames.get(gameId) ?? true) : true
        }));
        
        const updateData: UpdateConfigRequest = {
          name: configName !== editingConfig.name ? configName : undefined,
          games: games,
        };
        await configsApi.update(editingConfig.id, updateData);
      } else {
        const createData: CreateConfigRequest = {
          name: configName,
          gameIds: configGameIds,
        };
        await configsApi.create(createData);
      }
      setShowConfigModal(false);
      loadConfigs();
      loadAllGamesForConfigs();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error saving config');
    }
  };

  const handleDeleteConfig = async (id: string) => {
    if (!confirm('Are you sure you want to delete this config?')) return;
    try {
      await configsApi.delete(id);
      loadConfigs();
      loadAllGamesForConfigs();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error deleting config');
    }
  };

  const handleExportConfig = async (id: string) => {
    try {
      await configsApi.export(id);
    } catch (err: any) {
      setError(err.message || 'Error exporting config');
    }
  };

  const handleImportConfig = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    if (!file.name.endsWith('.json')) {
      setError('Only JSON files are supported');
      return;
    }

    try {
      setLoading(true);
      await configsApi.import(file);
      setError('');
      loadConfigs();
      loadAllGamesForConfigs();
    } catch (err: any) {
      setError(err.message || 'Error importing config');
    } finally {
      setLoading(false);
      event.target.value = '';
    }
  };

  const handleCreatePlatform = () => {
    setEditingPlatform(null);
    setPlatformName('');
    setPlatformDescription('');
    setPlatformUrl('');
    setPlatformConfigId('');
    setShowPlatformModal(true);
  };

  const handleEditPlatform = (platform: Platform) => {
    setEditingPlatform(platform);
    setPlatformName(platform.name);
    setPlatformDescription(platform.description || '');
    setPlatformUrl(platform.url || '');
    setPlatformConfigId(platform.configId);
    setShowPlatformModal(true);
  };

  const handleSavePlatform = async () => {
    try {
      if (editingPlatform) {
        const updateData: UpdatePlatformRequest = {
          name: platformName,
          description: platformDescription || undefined,
          url: platformUrl || undefined,
          configId: platformConfigId || undefined,
        };
        await platformsApi.update(editingPlatform.id, updateData);
      } else {
        const createData: CreatePlatformRequest = {
          name: platformName,
          description: platformDescription || undefined,
          url: platformUrl || undefined,
          configId: platformConfigId,
        };
        await platformsApi.create(createData);
      }
      setShowPlatformModal(false);
      loadPlatforms();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error saving platform');
    }
  };

  const handleDeletePlatform = async (id: string) => {
    if (!confirm('Are you sure you want to delete this platform?')) return;
    try {
      await platformsApi.delete(id);
      loadPlatforms();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error deleting platform');
    }
  };

  const toggleGameInConfig = (gameId: number) => {
    if (configGameIds.includes(gameId)) {
      setConfigGameIds(configGameIds.filter(id => id !== gameId));
      const newGames = new Map(configGames);
      newGames.delete(gameId);
      setConfigGames(newGames);
    } else {
      setConfigGameIds([...configGameIds, gameId]);
      const newGames = new Map(configGames);
      newGames.set(gameId, true);
      setConfigGames(newGames);
    }
  };

  const toggleGameEnabled = (gameId: number) => {
    if (!configGameIds.includes(gameId)) {
      return;
    }
    const newGames = new Map(configGames);
    const currentStatus = newGames.get(gameId) ?? true;
    newGames.set(gameId, !currentStatus);
    setConfigGames(newGames);
  };

  const handleEditGame = (game: Game) => {
    setEditingGame(game);
    setGameTitle(game.title || '');
    setGamePath(game.path || '');
    setGameCategory(game.category || '');
    setGameTags(game.tags || '');
    setShowGameModal(true);
  };

  const handleGameSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingGame) return;

    try {
      const updateData: UpdateGameRequest = {};
      
      const originalTitle = editingGame.title || '';
      const originalPath = editingGame.path || '';
      const originalCategory = editingGame.category || '';
      const originalTags = editingGame.tags || '';
      
      if (gameTitle !== originalTitle) {
        updateData.title = gameTitle || undefined;
      }
      if (gamePath !== originalPath) {
        updateData.path = gamePath || undefined;
      }
      if (gameCategory !== originalCategory) {
        updateData.category = gameCategory || undefined;
      }
      if (gameTags !== originalTags) {
        updateData.tags = gameTags || undefined;
      }

      if (Object.keys(updateData).length > 0) {
        await gamesApi.update(editingGame.id, updateData);
        setShowGameModal(false);
        loadGames();
      } else {
        setShowGameModal(false);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error updating game');
    }
  };

  const totalPages = Math.ceil(totalCount / 25);

  if (!hasAccess && !loading) {
    return (
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
    );
  }

  return (
    <div style={{ display: 'flex', height: '100vh', backgroundColor: '#f5f5f5' }}>
      {}
      <div style={{ width: '250px', backgroundColor: 'white', borderRight: '1px solid #ddd' }}>
        <div style={{ padding: '1rem', borderBottom: '1px solid #ddd' }}>
          <h2 style={{ fontSize: '1.25rem', fontWeight: 'bold' }}>Game Management</h2>
        </div>
        <div style={{ padding: '0.5rem' }}>
          <button
            onClick={() => setTab('games')}
            style={{
              width: '100%',
              textAlign: 'left',
              padding: '0.75rem 1rem',
              marginBottom: '0.5rem',
              border: 'none',
              borderRadius: '4px',
              cursor: 'pointer',
              backgroundColor: tab === 'games' ? '#dbeafe' : 'transparent',
              color: tab === 'games' ? '#1e40af' : '#333',
              fontWeight: tab === 'games' ? '500' : 'normal'
            }}
          >
            Games
          </button>
          <button
            onClick={() => setTab('configurations')}
            style={{
              width: '100%',
              textAlign: 'left',
              padding: '0.75rem 1rem',
              marginBottom: '0.5rem',
              border: 'none',
              borderRadius: '4px',
              cursor: 'pointer',
              backgroundColor: tab === 'configurations' ? '#dbeafe' : 'transparent',
              color: tab === 'configurations' ? '#1e40af' : '#333',
              fontWeight: tab === 'configurations' ? '500' : 'normal'
            }}
          >
            Configurations
          </button>
          <button
            onClick={() => setTab('platforms')}
            style={{
              width: '100%',
              textAlign: 'left',
              padding: '0.75rem 1rem',
              border: 'none',
              borderRadius: '4px',
              cursor: 'pointer',
              backgroundColor: tab === 'platforms' ? '#dbeafe' : 'transparent',
              color: tab === 'platforms' ? '#1e40af' : '#333',
              fontWeight: tab === 'platforms' ? '500' : 'normal'
            }}
          >
            Platforms
          </button>
        </div>
      </div>

      {/* Main content */}
      <div style={{ flex: 1, overflowY: 'auto' }}>
        {error && (
          <div style={{ margin: '1rem', padding: '1rem', backgroundColor: '#fee2e2', color: '#b91c1c', borderRadius: '4px' }}>
            {error}
          </div>
        )}

        {tab === 'games' && (
          <div style={{ padding: '2rem' }}>
            <h1 style={{ fontSize: '2rem', fontWeight: 'bold', marginBottom: '1.5rem' }}>Games</h1>

            {/* Search and filters */}
            <div style={{ backgroundColor: 'white', padding: '1rem', borderRadius: '8px', marginBottom: '1.5rem', boxShadow: '0 2px 4px rgba(0,0,0,0.1)' }}>
              <div style={{ display: 'flex', gap: '1rem', alignItems: 'flex-end', flexWrap: 'wrap' }}>
                <div style={{ flex: 1, minWidth: '200px' }}>
                  <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500' }}>Search by Title:</label>
                  <input
                    type="text"
                    value={searchTitle}
                    onChange={(e) => setSearchTitle(e.target.value)}
                    placeholder="Search by title..."
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
                <div style={{ flex: 1, minWidth: '200px' }}>
                  <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500' }}>Search by Path:</label>
                  <input
                    type="text"
                    value={searchPath}
                    onChange={(e) => setSearchPath(e.target.value)}
                    placeholder="Search by path..."
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
                <div style={{ width: '200px' }}>
                  <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500' }}>Provider:</label>
                  <input
                    type="text"
                    value={provider}
                    onChange={(e) => setProvider(e.target.value)}
                    placeholder="Filter by provider"
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
                <div style={{ width: '200px' }}>
                  <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500' }}>Category:</label>
                  <input
                    type="text"
                    value={category}
                    onChange={(e) => setCategory(e.target.value)}
                    placeholder="Filter by category"
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
                <button
                  onClick={handleSearchClick}
                  style={{
                    backgroundColor: '#007bff',
                    color: 'white',
                    padding: '0.75rem 1.5rem',
                    border: 'none',
                    borderRadius: '4px',
                    cursor: 'pointer',
                    fontSize: '1rem',
                    fontWeight: '500',
                    height: 'fit-content'
                  }}
                >
                  Search
                </button>
              </div>
            </div>

            {/* Games table */}
            {loading ? (
              <div style={{ textAlign: 'center', padding: '2rem' }}>Loading...</div>
            ) : (
              <>
                <div style={{ backgroundColor: 'white', borderRadius: '8px', overflow: 'hidden', boxShadow: '0 2px 4px rgba(0,0,0,0.1)' }}>
                  <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                    <thead>
                      <tr style={{ backgroundColor: '#f8f9fa' }}>
                        <th
                          style={{ padding: '1rem', textAlign: 'left', borderBottom: '1px solid #ddd', cursor: 'pointer' }}
                          onClick={() => handleSort('id')}
                        >
                          ID {sortBy === 'id' && (sortOrder === 'asc' ? '↑' : '↓')}
                        </th>
                        <th style={{ padding: '1rem', textAlign: 'left', borderBottom: '1px solid #ddd' }}>Title</th>
                        <th
                          style={{ padding: '1rem', textAlign: 'left', borderBottom: '1px solid #ddd', cursor: 'pointer' }}
                          onClick={() => handleSort('provider')}
                        >
                          Provider {sortBy === 'provider' && (sortOrder === 'asc' ? '↑' : '↓')}
                        </th>
                        <th
                          style={{ padding: '1rem', textAlign: 'left', borderBottom: '1px solid #ddd', cursor: 'pointer' }}
                          onClick={() => handleSort('category')}
                        >
                          Category {sortBy === 'category' && (sortOrder === 'asc' ? '↑' : '↓')}
                        </th>
                        <th style={{ padding: '1rem', textAlign: 'left', borderBottom: '1px solid #ddd' }}>Path</th>
                        <th style={{ padding: '1rem', textAlign: 'left', borderBottom: '1px solid #ddd' }}>Platforms</th>
                        <th style={{ padding: '1rem', textAlign: 'left', borderBottom: '1px solid #ddd' }}>Featured</th>
                        <th style={{ padding: '1rem', textAlign: 'left', borderBottom: '1px solid #ddd' }}>Actions</th>
                      </tr>
                    </thead>
                    <tbody>
                      {games.map(game => (
                        <tr key={game.id} style={{ borderBottom: '1px solid #ddd' }}>
                          <td style={{ padding: '1rem' }}>{game.id}</td>
                          <td style={{ padding: '1rem' }}>{game.title || '-'}</td>
                          <td style={{ padding: '1rem' }}>{game.provider || '-'}</td>
                          <td style={{ padding: '1rem' }}>{game.category || '-'}</td>
                          <td style={{ padding: '1rem' }}>{game.path || '-'}</td>
                          <td style={{ padding: '1rem' }}>
                            {game.platformNames.length > 0 ? game.platformNames.join(', ') : '-'}
                          </td>
                          <td style={{ padding: '1rem' }}>
                            {game.isFeatured ? (
                              <span style={{ padding: '0.25rem 0.5rem', backgroundColor: '#fef3c7', color: '#92400e', borderRadius: '4px' }}>
                                Featured
                              </span>
                            ) : (
                              '-'
                            )}
                          </td>
                          <td style={{ padding: '1rem' }}>
                            <button
                              onClick={() => handleEditGame(game)}
                              style={{
                                backgroundColor: '#007bff',
                                color: 'white',
                                padding: '0.5rem 1rem',
                                border: 'none',
                                borderRadius: '4px',
                                cursor: 'pointer',
                                fontSize: '0.875rem'
                              }}
                            >
                              Edit
                            </button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>

                {/* Pagination */}
                <div style={{ marginTop: '1.5rem', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <div style={{ fontSize: '0.9rem', color: '#666' }}>
                    Showing {(page - 1) * 25 + 1} to {Math.min(page * 25, totalCount)} of {totalCount} games
                  </div>
                  <div style={{ display: 'flex', gap: '0.5rem' }}>
                    <button
                      onClick={() => setPage(p => Math.max(1, p - 1))}
                      disabled={page === 1}
                      style={{
                        padding: '0.5rem 1rem',
                        border: '1px solid #ddd',
                        borderRadius: '4px',
                        cursor: page === 1 ? 'not-allowed' : 'pointer',
                        opacity: page === 1 ? 0.5 : 1,
                        backgroundColor: 'white'
                      }}
                    >
                      Previous
                    </button>
                    <span style={{ padding: '0.5rem 1rem', display: 'flex', alignItems: 'center' }}>
                      Page {page} of {totalPages}
                    </span>
                    <button
                      onClick={() => setPage(p => Math.min(totalPages, p + 1))}
                      disabled={page === totalPages}
                      style={{
                        padding: '0.5rem 1rem',
                        border: '1px solid #ddd',
                        borderRadius: '4px',
                        cursor: page === totalPages ? 'not-allowed' : 'pointer',
                        opacity: page === totalPages ? 0.5 : 1,
                        backgroundColor: 'white'
                      }}
                    >
                      Next
                    </button>
                  </div>
                </div>
              </>
            )}
          </div>
        )}

        {tab === 'configurations' && (
          <div style={{ padding: '2rem' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
              <h1 style={{ fontSize: '2rem', fontWeight: 'bold' }}>Configurations</h1>
              <div style={{ display: 'flex', gap: '0.75rem' }}>
                <label
                  style={{
                    backgroundColor: '#17a2b8',
                    color: 'white',
                    padding: '0.75rem 1.5rem',
                    border: 'none',
                    borderRadius: '4px',
                    cursor: 'pointer',
                    fontSize: '1rem',
                    display: 'inline-block'
                  }}
                >
                  Import Config
                  <input
                    type="file"
                    accept=".json"
                    onChange={handleImportConfig}
                    style={{ display: 'none' }}
                  />
                </label>
                <button
                  onClick={handleCreateConfig}
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
                  Create Config
                </button>
              </div>
            </div>
            {loading ? (
              <div style={{ textAlign: 'center', padding: '2rem' }}>Loading...</div>
            ) : (
              <div>
                {configs.map(config => (
                  <div key={config.id} style={{ backgroundColor: 'white', border: '1px solid #ddd', borderRadius: '8px', padding: '1rem', marginBottom: '0.5rem' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start' }}>
                      <div>
                        <div style={{ fontWeight: '500', fontSize: '1.1rem', marginBottom: '0.5rem' }}>{config.name}</div>
                        <div style={{ fontSize: '0.9rem', color: '#666' }}>
                          {config.gameIds.length} games, {config.platformNames.length} platforms
                        </div>
                      </div>
                      <div style={{ display: 'flex', gap: '0.5rem' }}>
                        <button
                          onClick={() => handleExportConfig(config.id)}
                          style={{
                            backgroundColor: '#6c757d',
                            color: 'white',
                            padding: '0.5rem 1rem',
                            border: 'none',
                            borderRadius: '4px',
                            cursor: 'pointer',
                            fontSize: '0.875rem'
                          }}
                        >
                          Export
                        </button>
                        <button
                          onClick={() => handleEditConfig(config)}
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
                        {config.platformNames.length === 0 && (
                          <button
                            onClick={() => handleDeleteConfig(config.id)}
                            style={{
                              backgroundColor: '#dc3545',
                              color: 'white',
                              padding: '0.5rem 1rem',
                              border: 'none',
                              borderRadius: '4px',
                              cursor: 'pointer'
                            }}
                          >
                            Delete
                          </button>
                        )}
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}

        {tab === 'platforms' && (
          <div style={{ padding: '2rem' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
              <h1 style={{ fontSize: '2rem', fontWeight: 'bold' }}>Platforms</h1>
              <button
                onClick={handleCreatePlatform}
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
                Create Platform
              </button>
            </div>
            {loading ? (
              <div style={{ textAlign: 'center', padding: '2rem' }}>Loading...</div>
            ) : (
              <div>
                {platforms.map(platform => (
                  <div key={platform.id} style={{ backgroundColor: 'white', border: '1px solid #ddd', borderRadius: '8px', padding: '1rem', marginBottom: '0.5rem' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start' }}>
                      <div>
                        <div style={{ fontWeight: '500', fontSize: '1.1rem', marginBottom: '0.5rem' }}>{platform.name}</div>
                        <div style={{ fontSize: '0.9rem', color: '#666' }}>Config: {platform.configName || 'N/A'}</div>
                        {platform.description && (
                          <div style={{ fontSize: '0.9rem', color: '#999', marginTop: '0.25rem' }}>{platform.description}</div>
                        )}
                        {platform.url && (
                          <div style={{ fontSize: '0.9rem', marginTop: '0.25rem' }}>
                            <a href={platform.url} target="_blank" rel="noopener noreferrer" style={{ color: '#007bff' }}>
                              {platform.url}
                            </a>
                          </div>
                        )}
                      </div>
                      <div style={{ display: 'flex', gap: '0.5rem' }}>
                        <button
                          onClick={() => handleEditPlatform(platform)}
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
                        <button
                          onClick={() => handleDeletePlatform(platform.id)}
                          style={{
                            backgroundColor: '#dc3545',
                            color: 'white',
                            padding: '0.5rem 1rem',
                            border: 'none',
                            borderRadius: '4px',
                            cursor: 'pointer'
                          }}
                        >
                          Delete
                        </button>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}
      </div>

      {/* Config Modal */}
      {showConfigModal && (
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
          onClick={() => setShowConfigModal(false)}
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
              {editingConfig ? 'Edit Config' : 'Create Config'}
            </h2>
            <div style={{ marginBottom: '1rem' }}>
              <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500' }}>Name:</label>
              <input
                type="text"
                value={configName}
                onChange={(e) => setConfigName(e.target.value)}
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
              <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500' }}>Games:</label>
              <div style={{ border: '1px solid #ddd', borderRadius: '4px', padding: '0.75rem', maxHeight: '300px', overflowY: 'auto' }}>
                {allGames.map(game => {
                  const isInConfig = configGameIds.includes(game.id);
                  const isEnabled = configGames.get(game.id) ?? true;
                  return (
                    <div key={game.id} style={{ marginBottom: '0.75rem', padding: '0.5rem', backgroundColor: isInConfig ? (isEnabled ? '#f0f9ff' : '#fef3c7') : 'transparent', borderRadius: '4px' }}>
                      <label style={{ display: 'flex', alignItems: 'center', cursor: 'pointer', marginBottom: isInConfig ? '0.5rem' : '0' }}>
                        <input
                          type="checkbox"
                          checked={isInConfig}
                          onChange={() => toggleGameInConfig(game.id)}
                          style={{ marginRight: '0.5rem' }}
                        />
                        <span style={{ fontSize: '0.9rem', flex: 1 }}>
                          {game.title || `Game #${game.id}`} ({game.provider || 'Unknown'})
                        </span>
                      </label>
                      {isInConfig && (
                        <label style={{ display: 'flex', alignItems: 'center', cursor: 'pointer', marginLeft: '1.5rem', fontSize: '0.85rem', color: '#666' }}>
                          <input
                            type="checkbox"
                            checked={isEnabled}
                            onChange={() => toggleGameEnabled(game.id)}
                            style={{ marginRight: '0.5rem' }}
                          />
                          <span>Enabled</span>
                        </label>
                      )}
                    </div>
                  );
                })}
              </div>
            </div>
            <div style={{ display: 'flex', gap: '1rem' }}>
              <button
                onClick={handleSaveConfig}
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
                onClick={() => setShowConfigModal(false)}
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

      {/* Platform Modal */}
      {showPlatformModal && (
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
          onClick={() => setShowPlatformModal(false)}
        >
          <div
            style={{
              backgroundColor: 'white',
              padding: '2rem',
              borderRadius: '8px',
              width: '90%',
              maxWidth: '500px',
              maxHeight: '90vh',
              overflowY: 'auto'
            }}
            onClick={(e) => e.stopPropagation()}
          >
            <h2 style={{ fontSize: '1.5rem', fontWeight: 'bold', marginBottom: '1rem' }}>
              {editingPlatform ? 'Edit Platform' : 'Create Platform'}
            </h2>
            <div style={{ marginBottom: '1rem' }}>
              <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500' }}>Name:</label>
              <input
                type="text"
                value={platformName}
                onChange={(e) => setPlatformName(e.target.value)}
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
                value={platformDescription}
                onChange={(e) => setPlatformDescription(e.target.value)}
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
            <div style={{ marginBottom: '1rem' }}>
              <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500' }}>URL:</label>
              <input
                type="url"
                value={platformUrl}
                onChange={(e) => setPlatformUrl(e.target.value)}
                maxLength={512}
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
              <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500' }}>Config:</label>
              <select
                value={platformConfigId}
                onChange={(e) => setPlatformConfigId(e.target.value)}
                required
                style={{
                  width: '100%',
                  padding: '0.75rem',
                  border: '1px solid #ddd',
                  borderRadius: '4px',
                  fontSize: '1rem',
                  boxSizing: 'border-box'
                }}
              >
                <option value="">Select a config</option>
                {configs.map(config => (
                  <option key={config.id} value={config.id}>
                    {config.name}
                  </option>
                ))}
              </select>
            </div>
            <div style={{ display: 'flex', gap: '1rem' }}>
              <button
                onClick={handleSavePlatform}
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
                onClick={() => setShowPlatformModal(false)}
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

      {/* Game Edit Modal */}
      {showGameModal && editingGame && (
        <div
          style={{
            position: 'fixed',
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            backgroundColor: 'rgba(0, 0, 0, 0.5)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            zIndex: 1000
          }}
          onClick={() => setShowGameModal(false)}
        >
          <div
            style={{
              backgroundColor: 'white',
              borderRadius: '8px',
              padding: '2rem',
              width: '90%',
              maxWidth: '600px',
              maxHeight: '90vh',
              overflowY: 'auto',
              boxShadow: '0 4px 6px rgba(0, 0, 0, 0.1)'
            }}
            onClick={(e) => e.stopPropagation()}
          >
            <h2 style={{ fontSize: '1.5rem', fontWeight: 'bold', marginBottom: '1.5rem' }}>Edit Game</h2>
            <form onSubmit={handleGameSubmit}>
              <div style={{ marginBottom: '1rem' }}>
                <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500' }}>Title:</label>
                <input
                  type="text"
                  value={gameTitle}
                  onChange={(e) => setGameTitle(e.target.value)}
                  maxLength={256}
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
                <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500' }}>Path:</label>
                <input
                  type="text"
                  value={gamePath}
                  onChange={(e) => setGamePath(e.target.value)}
                  maxLength={256}
                  style={{
                    width: '100%',
                    padding: '0.75rem',
                    border: '1px solid #ddd',
                    borderRadius: '4px',
                    fontSize: '1rem',
                    boxSizing: 'border-box'
                  }}
                />
                <div style={{ fontSize: '0.875rem', color: '#666', marginTop: '0.25rem' }}>
                  Path must be unique. If it conflicts, suffixes like -x1, -x2 will be added automatically.
                </div>
              </div>
              <div style={{ marginBottom: '1rem' }}>
                <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500' }}>Category:</label>
                <input
                  type="text"
                  value={gameCategory}
                  onChange={(e) => setGameCategory(e.target.value)}
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
              <div style={{ marginBottom: '1.5rem' }}>
                <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500' }}>Tags:</label>
                <input
                  type="text"
                  value={gameTags}
                  onChange={(e) => setGameTags(e.target.value)}
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
              <div style={{ display: 'flex', gap: '1rem', justifyContent: 'flex-end' }}>
                <button
                  type="button"
                  onClick={() => setShowGameModal(false)}
                  style={{
                    padding: '0.75rem 1.5rem',
                    border: '1px solid #ddd',
                    borderRadius: '4px',
                    backgroundColor: 'white',
                    cursor: 'pointer',
                    fontSize: '1rem'
                  }}
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  style={{
                    padding: '0.75rem 1.5rem',
                    border: 'none',
                    borderRadius: '4px',
                    backgroundColor: '#007bff',
                    color: 'white',
                    cursor: 'pointer',
                    fontSize: '1rem'
                  }}
                >
                  Save
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
